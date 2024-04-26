import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class PageInformationService {
    darkMode = false;

    constructor() { }

    updateMode() {
        this.darkMode = !this.darkMode;
    }

}
