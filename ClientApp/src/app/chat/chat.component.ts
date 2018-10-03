import { Component, HostListener, Inject } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';

/**
 * Mvc chat frontend
 */
@Component({
  selector: 'app-root',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent {
  public id: string;
  public messages: Array<[string, string, string]> = [];
  public messageText: string;
  public joinName: string;
  public users: ClientData[];
  public selectedUser: string;
  public errorMessage: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') private baseUrl: string) {
  }

  @HostListener('window:beforeunload', ['$event'])
  public onBeforeUnload() {
    this.leave();
  }

  // tslint:disable-next-line:use-life-cycle-interface
  public ngOnDestroy() {
    if (this.id) {
      this.leave();
    }
  }

  /**
   * Returns user name by identifier
   * @param id - user id
   * @param to - if true then return sender's user name else recipient's
   * @returns {string} - user name
   */
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

  /**
   * Handles HttpClient errors
   * @param error - specific error
   */
  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      this.errorMessage = error.error.message;
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong,
      this.errorMessage = `Backend returned code ${error.status}, body was: ${JSON.stringify(error.error)}`;
    }
  }

  /**
   * Joins to chat
   */
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

  /**
   * Disconnects user from the chat
   */
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

  /**
   * Sends a message to the chat
   */
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

  /**
   * Listens for incoming messages
   */
  private listen() {
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

        setTimeout( () => {
            // const receivedMessages = this.document.querySelector('#receivedMessages');
            const receivedMessages = document.getElementById('receivedMessages');
            receivedMessages.scrollTop = receivedMessages.scrollHeight;
          }, 0);

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
