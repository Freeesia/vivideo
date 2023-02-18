import { FirebaseOptions, initializeApp } from "firebase/app";
import { getAnalytics } from "firebase/analytics";
import { getAuth } from "firebase/auth";
import { initializeAppCheck, ReCaptchaV3Provider } from "firebase/app-check";
import axios from "axios";

export const firebaseOptions: FirebaseOptions = {
  apiKey: "AIzaSyCwqZqUUMbE6n5T3gdMFrLNW30VYvrKh20",
  authDomain: "vivideo.firebaseapp.com",
  databaseURL: "https://vivideo.firebaseio.com",
  projectId: "vivideo",
  storageBucket: "vivideo.appspot.com",
  messagingSenderId: "313434128507",
  appId: "1:313434128507:web:8246d5fac382681b6962de",
  measurementId: "G-TE802BVQ2K",
};
export const app = initializeApp(firebaseOptions);
getAnalytics(app);

export const auth = getAuth(app);

(async function () {
  const { data: info } = await axios.get<ClientInfo>("/api/info/clientinfo");
  initializeAppCheck(app, {
    provider: new ReCaptchaV3Provider(info.reCaptchaPublicKey),
    isTokenAutoRefreshEnabled: true,
  });
})();

interface ClientInfo {
  readonly reCaptchaPublicKey: string;
}
