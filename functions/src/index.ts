import { region } from "firebase-functions";
import { HttpsError } from "firebase-functions/lib/providers/https";
import { firestore, auth, initializeApp } from "firebase-admin";

const invitationCodes = "invitationCodes";

initializeApp();

const https = region("asia-northeast1").https;

export const checkInvitationCode = https.onCall(async (data, _) => {
  if (!data.invitationCode) {
    throw new HttpsError("invalid-argument", "auth/operation-not-allowed");
  }

  const codeRef = await firestore()
    .collection(invitationCodes)
    .doc(data.invitationCode)
    .get();

  const code = codeRef.data();
  if (!code) {
    throw new HttpsError("invalid-argument", "auth/invalid-code");
  }

  const count: number = code.remainingCount;
  if (count <= 0) {
    throw new HttpsError("invalid-argument", "auth/code-already-in-use");
  }

  return { status: "ok" };
});

export const signup = https.onCall(async (data, _) => {
  if (!data.email || !data.password || !data.name || !data.invitationCode) {
    throw new HttpsError("invalid-argument", "auth/operation-not-allowed");
  }

  const codeDoc = firestore()
    .collection(invitationCodes)
    .doc(data.invitationCode);

  const user = await firestore().runTransaction(async trans => {
    const codeRef = await trans.get(codeDoc);
    const code = codeRef.data();
    if (!code) {
      throw new HttpsError("invalid-argument", "auth/invalid-code");
    }
    const count: number = code.remainingCount;
    if (count <= 0) {
      throw new HttpsError("invalid-argument", "auth/code-already-in-use");
    }
    trans.update(codeDoc, {
      remainingCount: firestore.FieldValue.increment(-1)
    });

    return auth().createUser({
      email: data.email,
      emailVerified: false,
      password: data.password,
      displayName: data.name
    });
  });

  if (user) {
    await auth().setCustomUserClaims(user.uid, { invitationCodeVerified: true });
  }

  console.log({ userId: user.uid, code: data.invitationCode });

  return { status: "ok" };
});
