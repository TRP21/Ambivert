import { HttpClient } from '@angular/common/http';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';
import { Component, OnInit } from '@angular/core';
import { UserModel } from './_models/usersModel.model';
import { AccountService } from './_services/account.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'client';
 
  constructor(private accountService: AccountService) { }

  ngOnInit() {
    this.setCurrentUser();
  }



  setCurrentUser() {
    const UserString = localStorage.getItem('user');
    if (!UserString) return;
    const user: UserModel = JSON.parse(UserString);
    this.accountService.setCurrentUser(user);
  }
}
