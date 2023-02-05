import { app } from ".";
import {
  collection,
  connectFirestoreEmulator,
  doc,
  DocumentData,
  getDoc,
  getDocs,
  getFirestore,
  Query,
  query,
  QueryDocumentSnapshot,
  serverTimestamp,
  setDoc,
  SnapshotOptions,
  Timestamp,
  where,
} from "firebase/firestore";
import { HistoryVideo } from "@/model";
import { AuthModule } from "@/store";
import { assertIsDefined } from "@/utilities/assert";
import { dirname } from "path";
import { v5 as uuidv5, NIL } from "uuid";

const db = getFirestore(app);
if (process.env.NODE_ENV !== "production") {
  connectFirestoreEmulator(db, "localhost", 8888);
}

function getUserDocRef() {
  const user = AuthModule.user;
  assertIsDefined(user);
  return doc(db, "users", user.uid);
}
const getHistoriesRef = () =>
  collection(getUserDocRef(), "histories").withConverter<HistoryVideo>({ fromFirestore, toFirestore });
const getHistoryRef = (path: string) => doc(getHistoriesRef(), uuidv5(path, NIL));

export async function updatePlayingVideo(path: string, current: number) {
  const historyRef = getHistoryRef(path);
  await setDoc(historyRef, { path, current, lastUpdate: serverTimestamp() }, { merge: true });
}

export async function endPlayingVideo(path: string) {
  const historyRef = getHistoryRef(path);
  await setDoc(historyRef, { path, current: -1, lastUpdate: serverTimestamp() }, { merge: true });
}

export async function getHistory(path: string) {
  const historyRef = getHistoryRef(path);
  const doc = await getDoc(historyRef);
  if (!doc.exists()) {
    return null;
  } else {
    return doc.data();
  }
}

export async function getHistories(dir: string | undefined = undefined) {
  let colRef = getHistoriesRef() as Query<HistoryVideo>;
  if (dir) {
    colRef = query(colRef, where("dir", "==", dir));
  }
  const docs = await getDocs(colRef);
  return docs.docs.map(d => d.data());
}

function fromFirestore(snapshot: QueryDocumentSnapshot<DocumentData>, options?: SnapshotOptions): HistoryVideo {
  const data = snapshot.data(options);
  const lastUpdate = data.lastUpdate as Timestamp;
  return { path: data.path, current: data.current, lastUpdate: lastUpdate.toMillis() };
}

function toFirestore(modelObject: HistoryVideo): DocumentData {
  const { path, current } = modelObject;
  return { path, dir: dirname(path), current, lastUpdate: serverTimestamp() };
}
