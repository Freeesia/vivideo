import { setGlobalOptions } from "firebase-functions/v2";
import { HttpsError, onCall } from "firebase-functions/v2/https";
import { initializeApp } from "firebase-admin/app";
import { getAuth } from "firebase-admin/auth";
import { getFirestore, FieldValue } from "firebase-admin/firestore";

const invitationCodes = "invitationCodes";

initializeApp();
const db = getFirestore();
const auth = getAuth();
setGlobalOptions({
  region: "asia-northeast1",
});
interface InvitationCode {
  invitationCode: string;
}
interface SignupData extends InvitationCode {
  email: string;
  password: string;
}
export const checkinvitationcode = onCall<InvitationCode>(async ({ data: { invitationCode } }) => {
  if (!invitationCode) {
    throw new HttpsError("invalid-argument", "auth/operation-not-allowed");
  }

  const codeRef = await db.collection(invitationCodes).doc(invitationCode).get();

  const code = codeRef.data();
  if (!code) {
    throw new HttpsError("invalid-argument", "auth/invalid-code");
  }

  const count: number = code.remainingCount;
  if (count <= 0) {
    throw new HttpsError("invalid-argument", "auth/code-already-in-use");
  }
});

export const signup = onCall<SignupData>(async ({ data }) => {
  if (!data.email || !data.password || !data.invitationCode) {
    throw new HttpsError("invalid-argument", "auth/operation-not-allowed");
  }

  const codeDoc = db.collection(invitationCodes).doc(data.invitationCode);

  const user = await db.runTransaction(async trans => {
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
      remainingCount: FieldValue.increment(-1),
    });

    return auth.createUser({
      email: data.email,
      emailVerified: false,
      password: data.password,
    });
  });

  if (user) {
    await auth.setCustomUserClaims(user.uid, { invitationCodeVerified: true });
  }

  console.log({ userId: user.uid, code: data.invitationCode });
});

export const invite = onCall(async req => {
  const uid = req.auth?.uid;
  if (!uid && !process.env.FUNCTIONS_EMULATOR) {
    throw new HttpsError("unauthenticated", "認証されていません");
  }

  const codeRef = await db.collection(invitationCodes).add({
    remainingCount: 1,
    author: uid ?? "Unkown",
  });

  return codeRef.id;
});
