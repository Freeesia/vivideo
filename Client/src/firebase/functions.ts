import { app } from ".";
import { getFunctions, connectFunctionsEmulator, httpsCallableFromURL } from "firebase/functions";

export const functions = getFunctions(app, "asia-northeast1");
if (import.meta.env.DEV) {
  connectFunctionsEmulator(functions, "localhost", 5010);
}

export const invite = httpsCallableFromURL<void, string>(functions, "https://invite-uxogjp5z3q-an.a.run.app/invite");
export const checkCode = httpsCallableFromURL<{ invitationCode: string }>(
  functions,
  "https://checkInvitationCode-uxogjp5z3q-an.a.run.app/checkInvitationCode"
);
export const signup = httpsCallableFromURL(functions, "https://signup-uxogjp5z3q-an.a.run.app/signup");
