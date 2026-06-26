# Cybersecurity Awareness Chatbot

A comprehensive WPF-based cybersecurity awareness chatbot designed to educate South African
citizens about online safety practices. This application serves as an interactive virtual assistant
that educates users on identifying and mitigating cyber threats through conversational 
interactions, quizzes, and task management.

---

## Table of Contents

- [Overview]
- [Features]
- [Technical Requirements]
- [Installation]
- [Project Structure]
- [How to Use]
- [Features in Detail]
- [Database]
- [Troubleshooting]
- [Contributing]
- [License]

---

## Overview

In response to the significant rise in cyberattacks targeting individuals, businesses, and government institutions in South Africa, this chatbot serves as a "Cybersecurity Awareness Assistant." Its purpose is to simulate real-life scenarios where users might encounter cyber threats and provide guidance on avoiding common traps.

The application covers topics like:
- Phishing emails
- Safe password practices
- Recognizing suspicious links
- Privacy protection
- Two-factor authentication
- Social engineering awareness

---

## Features

### 1. Interactive Chatbot
- Keyword recognition for cybersecurity topics
- Random responses for engaging conversations
- Sentiment detection (worried, curious, frustrated)
- Memory and recall of user details
- Natural conversation flow with follow-up questions

### 2. Task Assistant
- Add cybersecurity-related tasks
- Set reminders for tasks
- Mark tasks as completed
- Delete tasks
- View all tasks with status

### 3. Cybersecurity Quiz
- 12+ multiple-choice questions
- Immediate feedback with explanations
- Score tracking
- Quiz history
- Mix of question types

### 4. Activity Log
- Track all user actions
- View recent activities
- Timestamp for each action
- Categories for different activities

### 5. Database Integration
- SQLite database for persistent storage
- Stores tasks, quiz results, and activity logs
- No external server required

---

## Technical Requirements

### Prerequisites
- **Operating System**: Windows 10/11
- **IDE**: Visual Studio 2022
- **.NET Framework**: .NET 6.0 or higher
- **Database**: SQLite (embedded)

### NuGet Packages Required
```xml
<PackageReference Include="System.Data.SQLite" Version="1.0.118" />
<PackageReference Include="System.Data.SQLite.Core" Version="1.0.118" />
```

---

## Installation

### Step 1: Clone or Download the Project
```bash
git clone https://github.com/yourusername/cybersecurity-chatbot.git
```

### Step 2: Open in Visual Studio
1. Open Visual Studio 2022
2. Click **Open a project or solution**
3. Navigate to the project folder and select `CybersecurityChatbot.sln`

### Step 3: Install NuGet Packages
```powershell
# In Package Manager Console
Install-Package System.Data.SQLite
Install-Package System.Data.SQLite.Core
```

### Step 4: Build and Run
1. Press `Ctrl + Shift + B` to build
2. Press `F5` to run with debugging
3. Or press `Ctrl + F5` to run without debugging

---

## Project Structure

```
CybersecurityChatbot/
├── Services/                          # Service layer
│   ├── ChatbotService.cs             # Main chatbot logic
│   ├── TaskService.cs                # Task management
│   ├── QuizService.cs                # Quiz functionality
│   ├── ActivityLogService.cs         # Activity logging
│   └── DatabaseService.cs            # Database operations
├── Models/                            # Data models
│   ├── ChatMessage.cs
│   ├── TaskItem.cs
│   ├── QuizQuestion.cs
│   ├── ActivityLogEntry.cs
│   └── UserProfile.cs
├── Converters/                        # Value converters
│   └── BooleanToVisibilityConverter.cs
├── Data/                              # Database directory
│   └── chatbot_data.db               # SQLite database (auto-created)
├── App.xaml                           # Application resources
├── App.xaml.cs                        # Application logic
├── MainWindow.xaml                    # Main UI
├── MainWindow.xaml.cs                 # Main UI logic
└── README.md                          # This file
```

---

## How to Use

### Getting Started
1. **Launch the application**
2. **Start chatting** by typing in the message box
3. **Explore features** using the tabs on the right panel

### Chat Commands

| Command | Description |
|---------|-------------|
| `Hello` / `Hi` | Greet the chatbot |
| `My name is [name]` | Set your name for personalized responses |
| `I'm interested in [topic]` | Set your favorite cybersecurity topic |
| `Tell me about [topic]` | Get information about cybersecurity topics |
| `Show activity log` | View recent actions |
| `Start quiz` | Begin the cybersecurity quiz |
| `Add task: [title]` | Create a new task |
| `Show tasks` | View all tasks |
| `Complete task [ID]` | Mark a task as completed |
| `Delete task [ID]` | Delete a task |
| `Exit` / `Goodbye` | End the conversation |

### Supported Topics
-  **Passwords** - Strong password creation and management
-  **Phishing** - Identifying and avoiding phishing attempts
-  **Privacy** - Protecting your personal information
-  **Scams** - Recognizing and avoiding scams
-  **2FA** - Two-factor authentication
-  **Updates** - Software update importance
-  **Social Engineering** - Human manipulation tactics

### Quiz Features
- Multiple-choice questions
- True/False questions
- Immediate feedback with explanations
- Score tracking
- Quiz history

---

## Database

The application uses SQLite for persistent storage with the following tables:

### Tasks Table
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Title | TEXT | Task title |
| Description | TEXT | Task description |
| ReminderDate | TEXT | Optional reminder date |
| IsCompleted | INTEGER | 0=Pending, 1=Completed |
| CreatedDate | TEXT | Creation timestamp |

### ActivityLog Table
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Action | TEXT | Action type |
| Description | TEXT | Action details |
| Timestamp | TEXT | When it happened |
| Category | TEXT | Action category |

### QuizResults Table
| Column | Type | Description |
|--------|------|-------------|
| Id | INTEGER | Primary key |
| Score | INTEGER | Number correct |
| TotalQuestions | INTEGER | Total questions |
| Date | TEXT | Quiz date |

---

## Troubleshooting

### Common Issues and Solutions

#### 1. SQLite DLL Not Found
```
System.DllNotFoundException: Unable to load DLL 'e_sqlite3'
```
**Solution**: Install `SourceGear.sqlite3` NuGet package or downgrade to System.Data.SQLite version 1.0.119

#### 2. Database File Not Created
```
SQLite error: no such table: Tasks
```
**Solution**: Run the application once - it will auto-create the database and tables

#### 3. DateTime Format Error
```
System.FormatException: String '' was not recognized as a valid DateTime
```
**Solution**: The DatabaseService.cs includes safe parsing methods that handle null/empty values

#### 4. File Not Found in Visual Studio
```
The document cannot be opened. It has been renamed, deleted, or moved.
```
**Solution**: Close the tab, delete .vs folder, reopen solution

#### 5. Build Errors
```
The name 'Services' does not exist in the current context
```
**Solution**: Add `using CybersecurityChatbot.Services;` to the file

---

## Development

### Adding New Cybersecurity Topics
1. Open `Services/ChatbotService.cs`
2. Add your topic to `InitializeKeywordResponses()`
3. Add follow-up questions to `InitializeTopicFollowUp()`

### Adding Quiz Questions
1. Open `Services/QuizService.cs`
2. Add new questions to `InitializeQuestions()`
3. Include: Question, Options, CorrectAnswerIndex, Explanation, Category

### Customizing UI
1. Open `App.xaml` for global styles
2. Modify `MainWindow.xaml` for layout changes
3. Colors can be changed in the ResourceDictionary

### Extending the Database
1. Add new tables in `DatabaseService.InitializeDatabase()`
2. Create CRUD methods for new tables
3. Update models in the `Models` folder

---

## Screenshots
![image alt](https://github.com/Thabang002/CybersecurityChatbot/blob/c1d10b980d205d512e959cf66eed7b78ace73130/Screenshot_2026-06-26_18-46-29.png)

### Main Chat Interface
- Chat window with message bubbles
- Input area with buttons
- User and bot messages with different colors

### Task Management Tab
- Add new tasks with descriptions
- View all tasks with status
- Mark tasks as complete

### Quiz Tab
- Start quiz button
- Question display with options
- Score and feedback

### Help Tab
- Command reference
- Topic list
- Usage instructions

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards
- Follow OOP principles
- Use meaningful variable names
- Add XML comments for public methods
- Handle exceptions appropriately
- Test your changes before submitting

---

## License

This project is developed for educational purposes as part of a Portfolio of Evidence (POE) for a programming course.

---

## Authors

- **Your Name** - *Initial work* - [YourGitHub](https://github.com/yourusername)

---

## Acknowledgments

- The Independent Institute of Education (Pty) Ltd
- Department of Cybersecurity, South Africa
- All contributors and testers

---

## Support

For issues, questions, or suggestions:
- Open an issue on GitHub
- Contact your course instructor
- Refer to the documentation

---

## Future Enhancements

-  Voice input/output
-  More quiz questions
-  Advanced NLP with AI/ML
-  Multi-language support (Afrikaans, Zulu, Xhosa)
-  Mobile app version
-  Cloud sync capabilities
-  Real-time cybersecurity news feed
-  Gamification with achievements

---

##  Version History

- **v1.0** - Initial release
  - Basic chatbot functionality
  - Keyword recognition
  - Task management
  - Quiz feature
  - Activity logging
  - SQLite database

---

##  Key Achievements

This project demonstrates:
-  Object-Oriented Programming principles
-  GUI development with WPF
-  Database integration with SQLite
-  Natural Language Processing simulation
-  Sentiment analysis
-  User memory and personalization
-  Task management system
-  Interactive quiz system
-  Activity logging
-  Error handling and validation
-  Clean, professional UI design

---

## References

- Pieterse, 2021 - Research on cyberattacks in South Africa
- Microsoft WPF Documentation
- System.Data.SQLite Documentation
- DeepSeeker, 2022 - Natural Language Processing in C#
- Gemini, 2023 - Sentiment Analysis Techniques

---

"Cybersecurity is everyone's responsibility. Stay safe online!"
