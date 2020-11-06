import firebase from "firebase/app";
import "firebase/analytics";
import "firebase/functions";

// Initialize Firebase
firebase.initializeApp({
  apiKey: "AIzaSyCwqZqUUMbE6n5T3gdMFrLNW30VYvrKh20",
  authDomain: "vivideo.firebaseapp.com",
  databaseURL: "https://vivideo.firebaseio.com",
  projectId: "vivideo",
  storageBucket: "vivideo.appspot.com",
  messagingSenderId: "313434128507",
  appId: "1:313434128507:web:8246d5fac382681b6962de",
  measurementId: "G-TE802BVQ2K",
});
firebase.analytics();

if (process.env.NODE_ENV !== "production") {
  firebase.app().functions("asia-northeast1").useEmulator("localhost", 5000);
}
