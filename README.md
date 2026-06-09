# LearnBase

> A web-based Learning Management & Practice Platform built to help students organize study materials, practice through active recall, and track performance over time.

[insert demo video or GIF here]

---

## Overview

LearnBase is a full-stack web application that combines **lesson management**, **exercise creation**, **structured practice sessions**, and **performance tracking** in a single, lightweight platform. It was built as an End-of-Year Project (PFA) for the Software Engineering program at EPI Digital School, Tunisia.

Unlike fragmented tools that separate note-taking from practice, LearnBase unifies the entire learning workflow -- letting users create organized lessons, build exercises in multiple formats, group them into practice sets, and run randomized practice sessions with measurable feedback.

---

## Key Features

| Feature | Description |
|---|---|
| **Lesson Management** | Create, edit, and organize lessons with typed notes and uploaded files (PDF, images, documents). Search, sort, import/export individual lessons or full datasets. |
| **Exercise Engine** | Create three exercise types -- Multiple Choice Questions (MCQ), Fill-in-the-Blank, and Flashcards. Tag exercises for flexible categorization. |
| **Practice Sets** | Group exercises manually or auto-generate sets from tags. Link sets to lessons for contextual access. |
| **Practice Sessions** | Run sessions in default or randomized order. Answer, skip, or end anytime. Real-time progress tracking with correct/incorrect/skipped counts. |
| **Performance History** | Review detailed session summaries with score percentages, per-question breakdowns, and historical trends. |
| **Data Portability** | Full export/import system using JSON -- backup individual items or your complete dataset. |
| **Secure Authentication** | JWT-based auth with secure password hashing and protected API endpoints. |

---

## Tech Stack

**Backend**
- ASP.NET Core 8 Web API
- Entity Framework Core (ORM)
- SQLite (lightweight, file-based database)
- JWT Authentication
- Swagger (API documentation & testing)

**Frontend**
- Angular 20
- TypeScript
- Angular Material (UI component library)
- HTML / CSS

**Tools**
- Visual Studio 2022 (backend IDE)
- Visual Studio Code (frontend editor)
- Git (version control)
- Draw.io (UML modeling)

---

## Architecture

The application follows a **Three-Tier Architecture** with a clean separation of concerns:

```
+-------------------------------------+
|      Presentation Layer             |
|    Angular 20 + Angular Material    |
+--------------+----------------------+
               | HTTP/REST + JSON
+--------------v----------------------+
|    Application / Business Layer     |
|    ASP.NET Core 8 -- Controllers,   |
|    DTOs, JWT Auth, Business Rules   |
+--------------+----------------------+
               | Entity Framework Core
+--------------v----------------------+
|      Data Access Layer              |
|    SQLite -- ORM, Migrations,       |
|    Data Integrity                   |
+-------------------------------------+
```

---

## Screenshots

### Authentication
[insert login/signup screenshot here]

### Lesson Management
[insert lesson list / lesson creation screenshot here]

### Exercise Creation
[insert exercise creation screenshot here -- MCQ, fill-in-blank, or flashcard]

### Practice Sets
[insert practice set list screenshot here]

### Practice Session
[insert practice session in progress screenshot here]

### Session Summary & History
[insert session summary / history list screenshot here]

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+)
- [Angular CLI](https://angular.io/cli)
- Git

### Clone the Repository
```bash
git clone https://github.com/samiKechiche/LearnBase.git
cd LearnBase
```

### Run the Backend
```bash
cd backend/LearnBase.API
dotnet restore
dotnet run
```
The API will start on `https://localhost:5001` (Swagger UI available at `/swagger`).

### Run the Frontend
```bash
cd frontend
npm install
ng serve
```
The application will be available at `http://localhost:4200`.

### Quick Start (Windows)
A batch file is included at the root for convenience:
```bash
start-learnbase.bat
```

---

## API Documentation

Once the backend is running, explore the full API documentation via Swagger:

```
https://localhost:5001/swagger
```

[insert Swagger UI screenshot here -- optional]

---

## Project Structure

```
LearnBase/
|
+-- backend/
|   +-- LearnBase.API/           # ASP.NET Core Web API
|   |   +-- Controllers/         # RESTful API controllers
|   |   +-- Models/              # Entity models
|   |   +-- Data/                # DbContext & migrations
|   |   +-- Services/            # Business logic & auth
|   |   +-- appsettings.json
|   +-- LearnBase.sln
|
+-- frontend/
|   +-- src/
|   |   +-- app/
|   |   |   +-- components/      # UI components
|   |   |   +-- services/        # API & auth services
|   |   |   +-- models/          # TypeScript interfaces
|   |   +-- ...
|   +-- angular.json
|
+-- start-learnbase.bat
+-- .gitignore
+-- LICENSE (MIT)
+-- README.md
```

---

## Future Improvements

- [ ] Spaced repetition algorithm integration
- [ ] Advanced long-term analytics and progress visualization
- [ ] Multimedia content support (images, audio, video within exercises)
- [ ] Collaborative deck sharing between users
- [ ] Mobile-responsive design or native mobile app
- [ ] Database migration from SQLite to PostgreSQL/SQL Server for production scale

---

## Team

This project was developed collaboratively by a team of three Software Engineering students as part of the End-of-Year Project at **EPI Digital School** (Academic Year 2025/2026).

| Team Member | Primary Responsibility |
|---|---|
| [**Sami Kechiche**](https://github.com/samiKechiche) | Exercise & Practice Subsystem -- exercise management, tagging, practice sets, randomized sessions, answer evaluation, session tracking, practice UI |
| [Youssef Nouira](https://github.com/youssefnouira20) | Lesson & Content Management -- lesson containers, file upload/storage, notes editor, lesson organization, search, lesson UI |
| [Saifedine Ltaief](https://github.com/saiefeltaief-2003) | Authentication & Data Portability -- JWT-based auth, password management, authorization middleware, export/import services |

All team members contributed to requirement analysis, system design, UML modeling, integration, and testing.

---

## License

This project is licensed under the MIT License -- see the [LICENSE](LICENSE) file for details.

---

## Acknowledgments

Developed at [EPI Digital School](https://www.epi-digital-school.com/), Sousse, Tunisia -- as part of the Software Engineering End-of-Year Project (PFA), Academic Year 2025/2026.

Special thanks to Mr. Firas Chouchene Hamila for guidance on project methodology and organization techniques, and to Mr. Adel Dahmane, Head of the Software Engineering Department, for continuous support throughout our academic journey.
