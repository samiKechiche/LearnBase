# LearnBase

> A full‑stack Learning Management & Practice Platform. Create organized lessons, build exercises in multiple formats, run active‑recall practice sessions, and track your performance — all in one place.

<p align="center">
  <video src="https://github.com/user-attachments/assets/be5aa0e2-0fec-4a23-b89f-c018cb83a7ac" controls width="90%"></video>
  <br/>
  <em>▶️ Full platform walkthrough (~4 min)</em>
</p>

---

## ✨ Features

| Feature | Description | Built by |
|---------|-------------|----------|
| **Lesson Management** | Create, edit, and organize lessons with typed notes and file uploads (PDF, images, documents). Search, sort, import/export. | Youssef |
| **Exercise Engine** | Three exercise types — Multiple Choice, Fill‑in‑the‑Blank, and Flashcards. Tag exercises for flexible categorization. | Sami |
| **Practice Sets** | Group exercises manually or auto‑generate from tags. Link sets to lessons for contextual access. | Sami |
| **Practice Sessions** | Run sessions in default or randomized order. Answer, skip, or end anytime. Real‑time progress tracking. | Sami |
| **Performance History** | Detailed session summaries with score percentages, per‑question breakdowns, and historical trends. | Sami |
| **Data Portability** | Full export/import system using JSON — back up individual items or your complete dataset. | Saifeddine |
| **Secure Authentication** | JWT‑based auth with secure password hashing and protected API endpoints. | Saifeddine |

---

## 📸 Screenshots

<table align="center">
  <tr>
    <td align="center" width="50%">
      <img src="assets/screenshots/Log%20in.png" width="100%" alt="Authentication" />
      <br/><sub>Authentication</sub>
    </td>
    <td align="center" width="50%">
      <img src="assets/screenshots/Lesson%20List.png" width="100%" alt="Lesson List" />
      <br/><sub>Lesson List</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <img src="assets/screenshots/Lesson%20Details.png" width="100%" alt="Lesson Details" />
      <br/><sub>Lesson Details</sub>
    </td>
    <td align="center" width="50%">
      <img src="assets/screenshots/Note%20Creation.png" width="100%" alt="Note Creation" />
      <br/><sub>Note Creation</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <img src="assets/screenshots/Exercise%20List.png" width="100%" alt="Exercise List" />
      <br/><sub>Exercise List</sub>
    </td>
    <td align="center" width="50%">
      <img src="assets/screenshots/Practice%20Set%20Creation%20%26%20List%20interface.png" width="100%" alt="Practice Sets" />
      <br/><sub>Practice Sets</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <img src="assets/screenshots/Start%20Practice%20Session.png" width="100%" alt="Start Practice Session" />
      <br/><sub>Start Practice Session</sub>
    </td>
    <td align="center" width="50%">
      <img src="assets/screenshots/Practice%20Session%20-%20Answer%20MCQ%20Exercise.png" width="100%" alt="Practice Session — MCQ" />
      <br/><sub>Practice Session — MCQ</sub>
    </td>
  </tr>
  <tr>
    <td align="center" width="50%">
      <img src="assets/screenshots/Practice%20Session%20History%20List.png" width="100%" alt="Session History" />
      <br/><sub>Session History</sub>
    </td>
    <td align="center" width="50%">
      <img src="assets/screenshots/Swagger%20UI.png" width="100%" alt="Swagger API Documentation" />
      <br/><sub>Swagger API Documentation</sub>
    </td>
  </tr>
</table>

---

## 🛠 Tech Stack

**Backend**
- ASP.NET Core 8 Web API
- Entity Framework Core (ORM)
- SQLite (file‑based database)
- JWT Authentication
- Swagger (API docs & testing)

**Frontend**
- Angular 20
- TypeScript
- Angular Material (UI components)
- HTML / CSS

**Tools**
- Visual Studio 2022 · VS Code · Git

---

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (v18+)
- [Angular CLI](https://angular.io/cli)

### 1. Clone the repository

```bash
git clone https://github.com/samiKechiche/LearnBase.git
cd LearnBase
```

### 2. Run the Backend

```bash
cd backend/LearnBase.API
dotnet restore
dotnet run
```

The API starts at `https://localhost:5001` — Swagger UI available at `/swagger`.

### 3. Run the Frontend

```bash
cd frontend
npm install
ng serve
```

The app will be available at `http://localhost:4200`.

> **Windows shortcut:** Double‑click `start-learnbase.bat` at the project root to launch both backend and frontend in one step.

---

## 📁 Project Structure

```
LearnBase/
├── backend/
│   └── LearnBase.API/
│       ├── Controllers/     # RESTful API controllers
│       ├── Models/          # Entity models
│       ├── Data/            # DbContext & migrations
│       └── Services/        # Business logic & auth
├── frontend/
│   └── src/app/
│       ├── components/      # UI components
│       ├── services/        # API & auth services
│       └── models/          # TypeScript interfaces
├── assets/
│   ├── screenshots/         # UI screenshots
│   └── videos/              # Demo video
├── start-learnbase.bat
├── .gitignore
├── LICENSE (MIT)
└── README.md
```

---

## 👥 Team

Team project built together with [Youssef Nouira](https://github.com/youssefnouira20) and [Saifeddine Eltaif](https://github.com/saiefeltaief-2003).

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).
