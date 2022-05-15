import { action, createModule } from "vuex-class-component";
import { User } from "firebase/auth";
import Axios from "axios";
import { auth } from "@/firebase";

const VuexModule = createModule({
  namespaced: "auth",
  strict: false,
});

export default class Auth extends VuexModule {
  private _user: User | null = null;

  get user() {
    return this._user;
  }

  @action
  async signOut() {
    await auth.signOut();
    this._user = null;
  }

  @action
  async isSignedIn() {
    let user = this.user;
    if (!user) {
      user = await new Promise<User | null>((res, rej) => {
        auth.onAuthStateChanged(async u => {
          const result = await u?.getIdTokenResult();
          res(result?.claims?.invitationCodeVerified ? u : null);
        }, rej);
      });
      if (!user) {
        auth.signOut();
      }
      this._user = user;
    }
    return user ? true : false;
  }

  @action
  async getAxios() {
    if (!this.user) {
      throw new Error("ログインされていません");
    }

    const token = await this.user.getIdToken();
    return Axios.create({
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
  }
}
