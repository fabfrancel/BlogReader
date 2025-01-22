import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';
import { DomSanitizer, SafeHtml } from "@angular/platform-browser";
import { firstValueFrom } from 'rxjs';

interface BlogSummary {
  title: string;
  items: FeedItem;
}

interface FeedItem {
  title: string;
  summary: string;
  publishDate: Date;
  link: string;
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

  async getPosts() {
    try {
      this.blogFeed = await firstValueFrom(this.http.get<BlogSummary[]>(environment.baseUrl + 'api/blogpost'));
    } catch (error) {
      console.error(error);
    }
  }

  getSanitizedHtml(Summary: string): SafeHtml {
    return this.sanitizer.bypassSecurityTrustHtml(Summary);
  }

  title = "Blog News"
}
