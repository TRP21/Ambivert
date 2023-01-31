import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators'
import { Member } from '../_models/member';
import { UserModel } from '../_models/usersModel.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseURl = 'https://localhost:5001/api/';
  private currentUserSource = new BehaviorSubject<UserModel | null>(null);
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  login(model: UserModel) {
    return this.http.post<UserModel>(this.baseURl + 'account/login', model).pipe(map((response: any) => {
      console.log(response); 
      const user = response;
      if (user) {
      this.setCurrentUser(user);
      }
    }))
  }

  setCurrentUser(user: UserModel) {
    localStorage.setItem('user', JSON.stringify(user))
    this.currentUserSource.next(user);
  }

  logOut() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

  register(model: UserModel) {
    return this.http.post<UserModel>(this.baseURl + 'account/register', model).pipe(
      map((user) => {
    this.setCurrentUser(user);
      }
      )

    )
  }

 
}
