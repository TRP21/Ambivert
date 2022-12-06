import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators'
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
      const user = response;
      if (user) {
        localStorage.setItem('user', JSON.stringify(user))
        this.currentUserSource.next(user);
      }
    }))
  }

  setCurrentUser(user:UserModel){
 this.currentUserSource.next(user);
  }

  logOut() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }

  register(model:UserModel){
    return this.http.post<UserModel>(this.baseURl+'account/register',model).pipe(
      map((user) =>{
        localStorage.setItem('user',JSON.stringify(user));
        this.currentUserSource.next(user);
      }
)

    )
  }
}
