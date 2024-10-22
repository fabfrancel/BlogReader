import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { WeatherForecastComponent } from './weather-forecast/weather-forecast.component';
import { HomeComponent } from './home/home.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { BlogReaderComponent } from './blog-reader/blog-reader.component';
import { SiteFooterComponent } from './site-footer/site-footer.component';

@NgModule({
  declarations: [
    AppComponent,
    WeatherForecastComponent,
    HomeComponent,
    NavMenuComponent,
    BlogReaderComponent,
    SiteFooterComponent
  ],
  imports: [
    BrowserModule, HttpClientModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
