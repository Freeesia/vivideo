import { initializeApp } from "firebase/app";
import { getAnalytics } from "firebase/analytics";
import { getFunctions, connectFunctionsEmulator } from "firebase/functions";

// Initialize Firebase
const app = initializeApp({
  apiKey: "AIzaSyCwqZqUUMbE6n5T3gdMFrLNW30VYvrKh20",
  authDomain: "vivideo.firebaseapp.com",
  databaseURL: "https://vivideo.firebaseio.com",
  projectId: "vivideo",
  storageBucket: "vivideo.appspot.com",
  messagingSenderId: "313434128507",
  appId: "1:313434128507:web:8246d5fac382681b6962de",
  measurementId: "G-TE802BVQ2K",
});
getAnalytics(app);

export const functions = getFunctions(app, "asia-northeast1");
if (process.env.NODE_ENV !== "production") {
  connectFunctionsEmulator(functions, "localhost", 5000);
}
