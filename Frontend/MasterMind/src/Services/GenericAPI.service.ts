import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";

@Injectable({
    providedIn: 'root'
})


// generic API caller, this class will do all the call (Get, Delete, Post, Put, Patch) to every API 
export class GenericAPIService{

    constructor(private http: HttpClient) { }

    // Specific Request Creator
    get<T>(url: string, options?: any, withCredential?: boolean, observe?: string) {
        return this.makeRequest<T>('GET', url, undefined, options, withCredential, observe);
    }

    delete<T>(url: string, options?: any, withCredential?: boolean, observe?: string) {
        return this.makeRequest<T>('DELETE', url, undefined, options, withCredential, observe);
    }

    post<T>(url: string, body: any, options?: any, withCredential?: boolean, observe?: string) { 
        return this.makeRequest<T>('POST', url, body, options, withCredential, observe);
    }

    patch<T>(url: string, body: any, options?: any, withCredential?: boolean, observe?: string) { 
        return this.makeRequest<T>('PATCH', url, body, options, withCredential, observe);
    }

    put<T>(url: string, body: any, options?: any, withCredential?: boolean, observe?: string) { 
        return this.makeRequest<T>('PUT', url, body, options, withCredential, observe);
    }



    // Generic Request Creator
    makeRequest<T>(method: string, url: string, body?: any, options?: any, withCredential?: boolean, observe?: string) {
        method = method.toUpperCase();

        // Checking if the body is present or not (it depend on the method of the call i wanna do)
        if (method == 'DELETE' || method == 'GET') {
            if (body) {
                throw new Error('Delete And Get do not Require A body');
            }
        }
        else if (!body) {
            throw new Error('Post, Put and Patch Require A Body for The request');
        }

        // Checking the presence of the Options
        if (!options) options = {};
        
        if (!options.params) options.params = {};
    

        // Checking if the Credential are needed
        if (withCredential) {
            const token = JSON.parse(window.localStorage.getItem('Token') || "{}");
            if (token) {
                if(body) body.token = token;
                else options.params.token = token;
            } else {
                options.withCredential = withCredential
            }
        }

        if (body) options.body = body;

        // To make the HTTPClient "observe" the body of the response we use the options.observe = 'body' so it will observe only the 
        // body of the response and not the other metadata for example
        if(!observe){
            observe = 'body';
        }
        options.observe = observe;

        return this.http.request<T>(method, url, options) as Observable<T>;
    }
}