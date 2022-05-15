import type { Query, CollectionReference, DocumentData, DocumentReference } from "firebase/firestore";
import { ActionContext } from "vuex";
import { FirestoreOptions } from "@posva/vuefire-core";
import { firestoreAction } from "vuexfire";

export interface FirestoreActionContext<S, R> extends ActionContext<S, R> {
  bindFirestoreRef(key: string, ref: Query | CollectionReference, options?: FirestoreOptions): Promise<DocumentData[]>;
  bindFirestoreRef(key: string, ref: DocumentReference, options?: FirestoreOptions): Promise<DocumentData>;
  unbindFirestoreRef(key: string): void;
}

const FirestoreAction = () => {
  return function (_target: any, _key: string, descriptor: PropertyDescriptor) {
    const delegate = descriptor.value;
    descriptor.value = function (payload: any) {
      const action = firestoreAction(context => delegate.call(context, payload));
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      return action(this);
    };
  };
};
export default FirestoreAction;
