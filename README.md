# A Simple Blog

## About
This is a simple blog that allows users to sign in and view posts by other users, as well as create their own posts.

It is a full-stack application that uses `ASP.NET` for the back end and `React.JS` (via `vite.js`) for the front end.
## Links
The documentation is located [here](docs/index.md).

### Requirements

#### Needed
- [ ] The user can create an account using their name, email address and password.
- [ ] The user can log in with their name/email address and password, and then be redirected to their profile.
- [ ] Both logged-in and guest users can see posts, unless they have been set to 'for registered users only' by the author.
- [ ] When logged in, users can create posts containing text and captions.
- [ ] The front end is localised (or can be in the future) and supports theming.
- [ ] Each user has a profile page where they can see their posts and name.
- [ ] There is a settings page that allows users to change their theme, delete their account, and change their password, username, and email address. 
- [ ] In the footer, links are displayed to GitHub, documentation and version information (Git).
- [ ] There is an 'About' page containing some information about the project.
- [ ] Errors are displayed in a stylised way that doesn't look out of place.
- [ ] Posts can also be viewed on a separate page via a URL (`/posts/<post_id>`).
#### Optional
- [ ] Logged-in users can click 'Mute' on a post to stop seeing any posts from that user.
- [ ] A user can also be an admin. If they are, they can delete posts and ban accounts.
- [ ] In the feed, you can hide posts so that they are not shown again.
- [ ] Markdown is allowed to format the posts.

#### Errors
- Login
    - When Username and password and incorrect show an error that they are incorrect.
    - When the username or password field is empy don't allow the user to click "login" and show a error.
    - If the account doesn't exist show an error explaining so and suggesting creating an account.
    - (Optional) If the account is banned show an error that this account has been banned.
- Sign Up
    - When username is already taken show that it is.
    - When email is already in use show an error as well.
    - When any field is empy don't allow the user to click "login" and show a error.
- Create new Post
    - When caption is empty don't allow the post to be posted.
- Settings
    - Error when trying to set your username to one that#s already taken or reserved.
    - Validate email addresses.


## Directory Structure
- [`docs/*`](docs) Documentation

- [`frontend/*`](frontend) Frontend Code

- [`backend/*`](backend) Backend Code

- [`api/*`](api) API Libarary (Typescript)

- [`scripts/*`](scripts) Useful scripts

## Development
Current Stage: Wireframe

Started: 14.11.2025 (DD-MM-YYYY)