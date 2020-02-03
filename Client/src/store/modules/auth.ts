import { VuexModule, Module, Mutation, Action } from "vuex-module-decorators";
import { auth, User } from "firebase/app";
import "firebase/auth";

@Module({ namespaced: true, name: "auth" })
export default class Auth extends VuexModule {
  user?: User | null;

  @Mutation
  private setUser(user: User | null) {
    this.user = user;
  }

  @Action
  async signOut() {
    await auth().signOut();
    this.context.commit("setUser", null);
  }

  @Action
  async isSignedIn() {
    let user = this.user;
    if (undefined === user) {
      user = await new Promise<User | null>((res, rej) => {
        auth().onAuthStateChanged(async u => {
          const result = await u?.getIdTokenResult();
          res(result?.claims?.invitationCodeVerified ? u : null);
        }, rej);
      });
      if (!user) {
        auth().signOut();
      }
      this.context.commit("setUser", user);
    }
    return user ? true : false;
  }
}
