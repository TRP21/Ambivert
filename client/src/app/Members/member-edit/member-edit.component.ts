import { NgForOfContext } from '@angular/common';
import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { iif } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { UserModel } from 'src/app/_models/usersModel.model';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm:NgForm|undefined;
  @HostListener('window:beforeunload',['$event'])
  unloadNotification($event:any)
  {
   if(this.editForm?.dirty)
   {
    $event.returnValue = true;
   }
  }   
  member: Member | undefined;
  user: UserModel | null = null;
  
  constructor(private accountService: AccountService, private memberService: MembersService, private toastr: ToastrService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => this.user = user
    })
  }


  ngOnInit(): void {
    this.loadMember()
  }

  loadMember() {
    if (!this.user) return;
    this.memberService.getMember(this.user.username).subscribe({
      next: member => this.member = member
    })
  }

  updateUser() {
   console.log(this.editForm?.value)
    this.memberService.updateMember(this.editForm?.value).subscribe(
      {
        next:_ =>{
          this.toastr.success("User is up dated");
          this.editForm?.reset(this.member)
        },
        error:error => {
          console.log(error)
          this.toastr.error(error.error);
        }
      });
  }
}
