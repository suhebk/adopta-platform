# Local Setup Steps — Adopta Platform

Use this local path:

```text
C:\Users\suheb\OneDrive\Desktop\Projects\Digital Adoption\adopta-platform
```

Because the path contains a space in `Digital Adoption`, always wrap it in quotes in PowerShell.

## 1. Create the GitHub repository

Create a new GitHub repository:

```text
Owner: suhebk
Repository name: adopta-platform
Visibility: Private
Add README: No
Add .gitignore: No
Add licence: No
```

Keep it empty initially.

## 2. Create the local folder and clone

```powershell
cd "C:\Users\suheb\OneDrive\Desktop"
mkdir "Projects"
mkdir "Projects\Digital Adoption"
cd "Projects\Digital Adoption"
git clone https://github.com/suhebk/adopta-platform.git
cd "C:\Users\suheb\OneDrive\Desktop\Projects\Digital Adoption\adopta-platform"
```

Check the path:

```powershell
pwd
```

Expected:

```text
C:\Users\suheb\OneDrive\Desktop\Projects\Digital Adoption\adopta-platform
```

## 3. Open in VS Code

```powershell
code .
```

## 4. Add this specification pack

Extract the Markdown ZIP into the repo root so the structure becomes:

```text
adopta-platform/
├── AGENTS.md
├── README.md
├── docs/
│   └── adopta/
│       ├── README.md
│       ├── 00-product-vision-and-competitive-analysis.md
│       ├── 01-user-requirements.md
│       ├── 02-functional-requirements.md
│       ├── 03-solution-architecture.md
│       ├── 04-wireframes.md
│       ├── 05-codex-visual-studio-build-prompt.md
│       ├── 06-review-retrofit-decisions-and-spec-updates.md
│       ├── setup/
│       │   └── LOCAL-SETUP-STEPS.md
│       └── sprints/
│           └── ADOPTA-SPRINT-1.md
├── src/
└── tests/
```

Store Word files separately under:

```text
docs/adopta/word/
```

## 5. Commit the baseline

```powershell
git status
git add .
git commit -m "Initial Adopta platform specification baseline"
git push
```

If Git asks for upstream:

```powershell
git push -u origin main
```

## 6. Create Sprint 1 branch

```powershell
git checkout -b adopta-sprint-1-saas-foundation
```

## 7. Start Codex safely

Paste the Sprint 1 prompt from:

```text
docs/adopta/sprints/ADOPTA-SPRINT-1.md
```

Codex must plan first and wait for approval before editing files.
