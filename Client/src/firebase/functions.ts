import { app } from ".";
import { getFunctions, connectFunctionsEmulator, httpsCallableFromURL } from "firebase/functions";

export const functions = getFunctions(app, "asia-northeast1");
let createUrl = (func: string) => `https://${func}-uxogjp5z3q-an.a.run.app/${func}`;
if (import.meta.env.DEV) {
  connectFunctionsEmulator(functions, "localhost", 5010);
  createUrl = (func: string) => `http://localhost:5010/${functions.app.options.projectId}/${functions.region}/${func}`;
}

export const invite = httpsCallableFromURL<void, string>(functions, createUrl("invite"));
export const checkCode = httpsCallableFromURL<{ invitationCode: string }>(functions, createUrl("checkinvitationcode"));
export const signup = httpsCallableFromURL(functions, createUrl("signup"));
