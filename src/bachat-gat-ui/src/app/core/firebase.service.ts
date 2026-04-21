import { Injectable } from '@angular/core';
import { initializeApp, FirebaseApp } from 'firebase/app';
import {
  getAuth, Auth,
  GoogleAuthProvider,
  signInWithPopup,
  UserCredential
} from 'firebase/auth';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FirebaseService {
  private app: FirebaseApp;
  private auth: Auth;

  constructor() {
    this.app = initializeApp(environment.firebase);
    this.auth = getAuth(this.app);
  }

  async signInWithGoogle(): Promise<string> {
    const provider = new GoogleAuthProvider();
    const result: UserCredential = await signInWithPopup(this.auth, provider);
    return result.user.getIdToken();
  }
}
