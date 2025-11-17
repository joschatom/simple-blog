# A simple Blog

## About
A simple blog that allows the user to sign into an account and view posts by other users and make posts themselves.

This is a fullstack application that uses `ASP.NET` for the backend and `React.JS` (via `vite.js`) for the frontend.

## Links
The documentation is located [here](docs/index.md).

## Requirements
- [ ] The user is able create an account with his name, e-mail, password.
- [ ] The user is able to login with his name/e-email and password and afterwards redirect back to the homepage.
- [ ] On the user both logged in and guest users can see posts created by users unless they have been set to `for registers users only` on the posts page.
- [ ] When logged in a user can create a post with text and a caption. They can also use Markdown in their posts for styling.
- [ ] The frontend is localized(or can be in the future)* and supports theming.
- [ ] A user can also be an admin, and if they are they can delete posts from users and ban accounts/users.
- [ ] In the feed you can Hide posts with cuases that posts to not be shown again.
- [ ] Logged in user can also click "mute" on a post to no longer see ayn posts from that user.
- [ ] Each users has a profile page where they see their posts and name.
- [ ] There's a settings page that allows users to change their theme, delete their account, change their password, username and email. 
- [ ] In the footer links are display to the GIthub and documenation and also version info(git).
- [ ] There's an "about" page with contains some info about the project.
- [ ] Errors are displayed ina stylized way that doesn#t look out of place.
- [ ] Posts can also be viewed on a seperate page via an url (`/posts/<postid>`).


## Directory Structure
- [`docs/*`](docs) Documentation

- [`docs/model`](docs/model) Application Models (development) [maybe?]

- [`frontend/*`](frontend) Forntend Code

- [`backend/*`](backend) Backend Code

- [`api/*`](api) API Libarary (Typescript)

- [`scripts/*`](scripts) Useful scripts

## Development
Current Stage: Wireframe

Started: 15.11.2025 (DD-MM-YYYY)