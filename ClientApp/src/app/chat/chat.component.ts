import { Component, HostListener, Inject } from '@angular/core';
import { HttpClient as Http, HttpErrorResponse, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent {
  public id: string;
  public baseUrl: string;
  public messages: Array<[string, string, string]> = [];
  public messageText: string;
  public joinName: string;
  public http: Http;
  public users: ClientData[];
  public selectedUser: string;
  public errorMessage: string;

  constructor(http: Http, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
    this.http = http;
  }

  @HostListener('window:beforeunload', ['$event'])
  public onBeforeUnload() {
    this.leave();
  }

  // tslint:disable-next-line:use-life-cycle-interface
  ngOnDestroy() {
    if (this.id) {
      this.leave();
    }
  }

  public getUserName(id: string, to: boolean): string {
    if (id) {
      if (this.users) {
        const user = this.users.find((v) => v.key === id);

        if (user) {
            return user.value.name;
        }
      }

      return `[${id}]`;
    }

    return to ? 'All' : 'Server';
  }

  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      this.errorMessage = error.error.message;
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong,
      this.errorMessage = `Backend returned code ${error.status}, body was: ${error.error}`;
    }
  }

  public join() {
    delete this.errorMessage;
    this.users = [];
    delete this.id;

    const httpOptions = {
        headers: new HttpHeaders({
          'Content-Type':  'application/json',
        })
    };

    this.http.post<string>(this.baseUrl + 'api/Chat/Join', `"${this.joinName}"`, httpOptions).subscribe(result => {
        this.id = result;
        if (!this.selectedUser) {
          this.selectedUser = ''; // All
        }
        this.listen();
    }, error => this.handleError(error));
  }

  public leave() {
      delete this.errorMessage;

    const httpOptions = {
      headers: new HttpHeaders({
        'Content-Type': 'application/json',
      })
    };

    this.http.post(this.baseUrl + 'api/Chat/Leave', `"${this.id}"`, httpOptions).subscribe(_ => {
        delete this.id;
        this.users = [];
    }, error => this.handleError(error));
  }

  public sendMessage() {
    delete this.errorMessage;

    if (!this.messageText) { return; }

    const msg: Message = {
        type: MessageType.TextMessage,
        from_id: this.id,
        to_id: this.selectedUser,
        data: {text: this.messageText},
    };
    this.messageText = '';
    this.http.post(this.baseUrl + 'api/Chat/Send', msg).subscribe(_ => {/**nothing to do */
    }, error => this.handleError(error));
  }

  public listen() {
    if (!this.id) {
      this.errorMessage = 'You has left the chat.';
      return;
    }
    this.http.get<Message[]>(this.baseUrl + 'api/Chat/Listen/' + this.id).subscribe(result => {
        const messages = result;

        let lastListOfUsers;
        messages.forEach((m) => {
            switch (m.type) {
                case MessageType.TextMessage:
                    this.messages.push([this.getUserName(m.from_id, false), this.getUserName(m.to_id, true), m.data.text || '']);
                    break;

                case MessageType.UsersList:
                    lastListOfUsers = m.data;
                    break;
            }
        });

        if (lastListOfUsers) {
            this.users = lastListOfUsers;
        }

        // Tail call optimization should to work here
        return this.listen();
    }, error => this.handleError(error));
  }
}

interface Client {
    name: string;
}
interface ClientData {
    key: string;
    value: Client;
}

enum MessageType {UsersList, TextMessage}

interface Message {
    type: MessageType;
    from_id: string;
    to_id: string;
    data: any;
}
