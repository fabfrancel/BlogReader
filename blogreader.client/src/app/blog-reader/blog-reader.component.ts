import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { DomSanitizer, SafeHtml } from "@angular/platform-browser";

interface BlogSummary {
  Id: number;
  Title: string;
  Items: FeedItem;
}

interface FeedItem {
  Id: number;
  Title: string;
  Summary: string;
  PublishDate: Date;
  Link: string;
}

@Component({
  selector: 'app-blog-reader',
  templateUrl: './blog-reader.component.html',
  styleUrl: './blog-reader.component.css'
})
export class BlogReaderComponent implements OnInit {


  public blogFeed?: BlogSummary[];

  constructor(private http: HttpClient, private sanitizer: DomSanitizer) { }

  ngOnInit() {
    this.getPosts();
  }

  getPosts() {
    this.http.get<BlogSummary[]>(environment.baseUrl + 'api/blogpost').subscribe(
      result => { this.blogFeed = result },
      error => { console.error(error) }
    )
  }

  getSanitizedHtml(Summary: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(Summary);
  }

  title = "Blog News"
}

