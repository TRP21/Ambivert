import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-test-errors',
  templateUrl: './test-errors.component.html',
  styleUrls: ['./test-errors.component.css']
})
export class TestErrorsComponent implements OnInit {
  validationErrors: string[] =[]
  constructor(private http: HttpClient) { }

  ngOnInit(): void {
  }
  baseURl = "https://localhost:5001/api";
  get404Error() {
    this.http.get(this.baseURl + '/buggy/not-found').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get400Error() {
    this.http.get(this.baseURl + '/buggy/bad-request').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get401Error() {
    this.http.get(this.baseURl + '/buggy/auth').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }

  get400ValidationError() {
    this.http.post(this.baseURl + '/account/register', {}).subscribe({
      next: response => console.log(response),
      error: error => {
        console.log(error);
        this.validationErrors = error;

      }
    })
  }

  get500Error() {
    this.http.get(this.baseURl + '/buggy/server-error').subscribe({
      next: response => console.log(response),
      error: error => console.log(error)
    })
  }
}
