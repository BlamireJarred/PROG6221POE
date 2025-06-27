using PROG_POE_ChatBot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CyberAssistant
{
    //chat bot input and output for add task
    public class ChatResponse
    {
        public string Message { get; set; }
        public TaskItem CreatedTask { get; set; }
        public bool AwaitingReminder { get; set; } = false;
    }

    //chat bot input and output for quiz
    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; } // For MCQs (optional for True/False)
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
        public bool IsTrueFalse { get; set; }
    }

    //chatbot logic
    public class ChatBot
    {
        //chatbot variables
        private string userName;
        private string currentTopic = null;
        private string favoriteTopic = null;
        private Random rand = new Random();

        //task variables
        private bool awaitingTaskTitle = false;
        private bool awaitingDescription = false;
        private string pendingTaskTitle;
        private string pendingTaskDescription;

        //quiz variables
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int currentQuizIndex = 0;
        private int quizScore = 0;
        private bool isInQuiz = false;
        private bool awaitingQuizAnswer = false;

        // Keywords for detecting user sentiment
        string[] worriedKeywords = { "worried", "anxious", "nervous", "concerned" };

        // Predefined response variations
        string[] howAreYouResponses = {
                "I'm a bot, so I don't have feelings, but thanks for asking!",
                "Running smoothly in the digital realm!",
                "All systems operational — how can I help you today?",
                "I'm functioning as expected. How can I assist?"
            };

        string[] purposeResponses = {
                "I help users understand basic cybersecurity concepts.",
                "My main goal is to educate people about staying safe online.",
                "I was designed to answer your cybersecurity-related questions.",
                "Think of me as your digital safety assistant!"
            };

        string[] passwordResponses = {
                "Always use a strong, unique password with a mix of characters.",
                "Never reuse passwords across different sites — it’s risky!",
                "Consider using a password manager to generate and store secure passwords.",
                "Avoid using personal info like birthdays or pet names in passwords."
            };

        string[] phishingResponses = {
                "Phishing is when attackers trick you into giving away personal info.",
                "Beware of emails that look urgent or suspicious — they could be phishing.",
                "Legit companies never ask for your password via email.",
                "Always verify links before clicking — hover to see where they lead."
            };

        string[] browsingResponses = {
                "Avoid suspicious websites and don't click on unknown links.",
                "Use a secure browser and enable pop-up blockers.",
                "Keep your browser and plugins up to date to prevent exploits.",
                "Look for 'https://' in the URL for secure sites."
            };

        string[] scamResponses = {
                "Scams often come through email or phone calls — always verify the source.",
                "Never share personal or financial info unless you're sure who you're talking to.",
                "Scammers try to create urgency — take your time and double-check.",
                "If something sounds too good to be true, it probably is — stay alert for scams."
            };

        string[] privacyResponses = {
                "Limit the amount of personal information you share online.",
                "Review your privacy settings on social media and apps regularly.",
                "Use encrypted messaging apps to protect your conversations.",
                "Be cautious when giving apps permission to access your location or contacts."
            };

        // "More info" arrays for each topic
        string[] morePasswordInfo = {
                "Using multi-factor authentication adds an extra layer of security.",
                 "Try to change your passwords every few months.",
                "Avoid using the same password across multiple accounts.",
                "Password managers can generate and store complex passwords safely."
            };

        string[] morePhishingInfo = {
                "Be skeptical of emails with spelling errors or urgent demands.",
                "Never download attachments from unknown senders.",
                "Banks and companies will never ask for your password by email.",
                "Phishing websites often look almost identical — double-check the URL."
            };

        string[] moreBrowsingInfo = {
                "Install browser extensions that block ads and trackers.",
                "Use private or incognito mode when needed.",
                "Avoid entering sensitive info on public Wi-Fi unless using a VPN.",
                "Clear cookies and cache regularly to protect your privacy."
            };

        string[] moreScamInfo = {
                "Online scams may impersonate trusted brands — always verify URLs.",
                "Avoid giving out personal info over the phone or text unless verified.",
                "Fake job offers and lottery wins are common scam tactics.",
                "Use reverse image search to check if a profile or item is fake."
            };

        string[] morePrivacyInfo = {
                "Be cautious with what you post on social media — it can be used against you.",
                "Review app permissions and disable unnecessary access.",
                "Use encrypted email and messaging services for added privacy.",
                "Don’t overshare personal details on forums or websites."
            };

        //quiz questions
        private void LoadQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                //q1
                new QuizQuestion {
                    Question = "What is phishing?",
                    Options = new[] { "A) A fishing sport", "B) A scam to steal personal info", "C) A type of malware", "D) A secure website protocol" },
                    CorrectAnswer = "B",
                    Explanation = "Phishing tricks users into giving personal info through fake messages.",
                    IsTrueFalse = false
                },

                //q2
                new QuizQuestion {
                    Question = "True or False: You should reuse passwords across multiple sites.",
                    CorrectAnswer = "False",
                    Explanation = "Reusing passwords increases your risk if one site is compromised.",
                    IsTrueFalse = true
                },

                //q3
                new QuizQuestion {
                    Question = "Which of the following is a strong password?",
                    Options = new[] {
                        "A) password123",
                        "B) 12345678",
                        "C) MyDogIsCool!",
                        "D) qwerty"
                    },
                    CorrectAnswer = "C",
                    Explanation = "Strong passwords use a mix of letters, numbers, and symbols.",
                    IsTrueFalse = false
                },

                //q4
                new QuizQuestion {
                    Question = "True or False: Public Wi-Fi is always safe to use for online banking.",
                    CorrectAnswer = "False",
                    Explanation = "Public Wi-Fi can be insecure and vulnerable to man-in-the-middle attacks.",
                    IsTrueFalse = true
                },

                //q5
                new QuizQuestion {
                    Question = "What should you check before clicking a link in an email?",
                    Options = new[] {
                        "A) If the email looks professional",
                        "B) If it’s from your boss",
                        "C) The actual URL behind the link",
                        "D) If it uses capital letters"
                    },
                    CorrectAnswer = "C",
                    Explanation = "Always hover over links to verify the real URL.",
                    IsTrueFalse = false
                },

                //q6
                new QuizQuestion {
                    Question = "True or False: Antivirus software protects against all cyber threats.",
                    CorrectAnswer = "False",
                    Explanation = "Antivirus helps, but no tool is perfect. Good habits are essential too.",
                    IsTrueFalse = true
                },

                //q7
                new QuizQuestion {
                    Question = "Which of these is an example of phishing?",
                    Options = new[] {
                        "A) A system update notification",
                        "B) An email from 'support@bank-login.com' asking for your password",
                        "C) A password change reminder from your system",
                        "D) A new Wi-Fi connection prompt at home"
                    },
                    CorrectAnswer = "B",
                    Explanation = "Phishing emails often use fake addresses to trick users into sharing info.",
                    IsTrueFalse = false
                },

                //q8
                new QuizQuestion {
                    Question = "What is social engineering?",
                    Options = new[] {
                        "A) A type of computer virus",
                        "B) A way to build secure systems",
                        "C) Manipulating people into giving up confidential info",
                        "D) Engineering on social media platforms"
                    },
                    CorrectAnswer = "C",
                    Explanation = "Social engineering relies on human error to breach systems.",
                    IsTrueFalse = false
                },

                //q9
                new QuizQuestion {
                    Question = "True or False: It's safe to download files from any email attachment.",
                    CorrectAnswer = "False",
                    Explanation = "Only download attachments from trusted, verified sources.",
                    IsTrueFalse = true
                },

                //q10
                new QuizQuestion {
                    Question = "What is a good practice for secure web browsing?",
                    Options = new[] {
                        "A) Clicking ads on every site",
                        "B) Using incognito mode all the time",
                        "C) Looking for HTTPS in the address bar",
                        "D) Accepting all cookies immediately"
                    },
                    CorrectAnswer = "C",
                    Explanation = "HTTPS ensures your connection is encrypted and more secure.",
                    IsTrueFalse = false
                }
            };
        }

        public ChatBot(string name)
        {
            userName = name;
        }

        public string ProcessChat(string input)
        {
            input = input.ToLower().Trim();

            if (string.IsNullOrWhiteSpace(input))
                return "I didn’t quite understand that. Try typing 'help' for ideas.";

            if (input.Contains("exit") || input.Contains("quit"))
                return $"Goodbye, {userName}! Stay safe online.";

            if (input.Contains("i'm interested in") || input.Contains("i am interested in"))
            {
                int index = input.Contains("i'm interested in")
                    ? input.IndexOf("i'm interested in") + "i'm interested in".Length
                    : input.IndexOf("i am interested in") + "i am interested in".Length;

                favoriteTopic = input.Substring(index).Trim();
                return $"I'll remember that you're interested in {favoriteTopic}. Good choice!";
            }

            if (input.Contains("favorite") || input.Contains("topic"))
            {
                return favoriteTopic != null
                    ? $"You mentioned interest in {favoriteTopic} earlier."
                    : "I don't think you've mentioned a favorite topic yet.";
            }

            // Sentiment check (e.g., worriedKeywords)
            if (worriedKeywords.Any(word => input.Contains(word)))
            {
                string sentimentMessage = "It's okay to feel worried. Let's learn how to stay protected online.\n";

                if (input.Contains("scam"))
                {
                    currentTopic = "scam";
                    return sentimentMessage + scamResponses[rand.Next(scamResponses.Length)];
                }

                return sentimentMessage;
            }

            // Keyword topic handling
            if (input.Contains("how are you"))
                return howAreYouResponses[rand.Next(howAreYouResponses.Length)];

            if (input.Contains("what is your purpose") || input.Contains("what can you do"))
                return purposeResponses[rand.Next(purposeResponses.Length)];

            if (input.Contains("password"))
            {
                currentTopic = "password";
                return passwordResponses[rand.Next(passwordResponses.Length)];
            }

            if (input.Contains("phishing"))
            {
                currentTopic = "phishing";
                return phishingResponses[rand.Next(phishingResponses.Length)];
            }

            if (input.Contains("scam"))
            {
                currentTopic = "scam";
                return scamResponses[rand.Next(scamResponses.Length)];
            }

            if (input.Contains("privacy"))
            {
                currentTopic = "privacy";
                return privacyResponses[rand.Next(privacyResponses.Length)];
            }

            if (input.Contains("more") || input.Contains("details") || input.Contains("explain"))
            {
                switch (currentTopic)
                {
                    case "password":
                        return morePasswordInfo[rand.Next(morePasswordInfo.Length)];
                    case "phishing":
                        return morePhishingInfo[rand.Next(morePhishingInfo.Length)];
                    case "scam":
                        return moreScamInfo[rand.Next(moreScamInfo.Length)];
                    case "privacy":
                        return morePrivacyInfo[rand.Next(morePrivacyInfo.Length)];
                    default:
                        return "What topic would you like more info about?";
                }
            }

            if (input.Contains("help") || input.Contains("what can i ask"))
            {
                return "You can ask me about: password safety, phishing, scams, privacy, or say 'more info' for deeper tips.";
            }

            return "I didn’t quite understand that. Could you rephrase?";
        }

        //helper class for task date time
        private DateTime? ParseNaturalDate(string input)
        {
            input = input.ToLower().Trim();

            if (input.Contains("tomorrow")) return DateTime.Now.Date.AddDays(1);
            if (input.Contains("today")) return DateTime.Now.Date;

            var relativeMatch = Regex.Match(input, @"(?:in|after|within)?\s*(\d+)\s*(second|seconds|minute|minutes|hour|hours|day|days)", RegexOptions.IgnoreCase);
            if (relativeMatch.Success)
            {
                int value = int.Parse(relativeMatch.Groups[1].Value);
                string unit = relativeMatch.Groups[2].Value;

                switch (unit)
                {
                    case "second":
                    case "seconds":
                        return DateTime.Now.AddSeconds(value);

                    case "minute":
                    case "minutes":
                        return DateTime.Now.AddMinutes(value);

                    case "hour":
                    case "hours":
                        return DateTime.Now.AddHours(value);

                    case "day":
                    case "days":
                        return DateTime.Now.AddDays(value);

                    default:
                        return null;
                }
            }

            if (DateTime.TryParse(input, out DateTime parsedDate))
            {
                if (parsedDate < DateTime.Now.Date)
                    parsedDate = parsedDate.AddYears(1);
                return parsedDate;
            }

            return null;
        }

        private string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalDays >= 1) return $"{(int)span.TotalDays} day(s)";
            if (span.TotalHours >= 1) return $"{(int)span.TotalHours} hour(s)";
            return $"{(int)span.TotalMinutes} minute(s)";
        }

        private string FormatQuestion(QuizQuestion q)
        {
            return q.IsTrueFalse
                ? $"{q.Question} (True/False)"
                : $"{q.Question}\n" + string.Join("\n", q.Options);
        }

        //allows chatbot to process add task and adds task to a textbox
        public ChatResponse ProcessInput(string input, List<TaskItem> taskList)
        {
            input = input.Trim();
            var response = new ChatResponse();

            // NLP for viewing the log
            if (input.Contains("activity log") || input.Contains("what have i done"))
            {
                var recentLog = ActivityLogger.GetRecentLog();
                response.Message = recentLog.Count == 0
                    ? "There's no activity yet."
                    : "Here's a summary of recent actions:\n" +
                      string.Join("\n", recentLog.Select((entry, index) => $"{index + 1}. {entry}"));
                return response;
            }


            //Full sentence add task with description
            if (input.ToLower().Contains("add") && input.ToLower().Contains("task") && input.ToLower().Contains("description"))
            {
                var titleMatch = Regex.Match(input, @"add task (.*?)(,| description:| desc:)", RegexOptions.IgnoreCase);
                var descMatch = Regex.Match(input, @"(description:|desc:)\s*(.*?)(,|$)", RegexOptions.IgnoreCase);

                string title = titleMatch.Success ? titleMatch.Groups[1].Value.Trim() : "Untitled Task";
                string description = descMatch.Success ? descMatch.Groups[2].Value.Trim() : "No description";

                var task = new TaskItem
                {
                    Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title),
                    Description = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(description),
                    ReminderDate = null,
                    IsCompleted = false
                };

                response.CreatedTask = task;
                response.Message = $"Task \"{task.Title}\" added with description \"{task.Description}\".";
                ActivityLogger.Log($"Task added: \"{task.Title}\"");
                return response;
            }

            //Reminder setting 
            if (input.ToLower().Contains("remind") || input.ToLower().Contains("set a reminder"))
            {
                // Relative: "remind me to do homework in 3 days"
                var relativeMatch = Regex.Match(input, @"remind(?: me)?(?: to)? (.*?) in (.*)", RegexOptions.IgnoreCase);
                if (relativeMatch.Success)
                {
                    string taskText = relativeMatch.Groups[1].Value.Trim();
                    string timeText = relativeMatch.Groups[2].Value.Trim();

                    var matchingTask = taskList.FirstOrDefault(t => t.Title.ToLower().Contains(taskText.ToLower()));
                    DateTime? reminderDate = ParseNaturalDate(timeText);

                    if (matchingTask != null && reminderDate.HasValue)
                    {
                        matchingTask.ReminderDate = reminderDate;
                        response.CreatedTask = matchingTask;
                        response.Message = $"Reminder set for \"{matchingTask.Title}\" in {FormatTimeSpan(reminderDate.Value - DateTime.Now)}.";
                        ActivityLogger.Log($"Reminder added: \"{matchingTask.Title}\"");
                    }
                    else
                    {
                        response.Message = matchingTask == null
                            ? $"Task \"{taskText}\" not found."
                            : $"Could not understand reminder time: \"{timeText}\".";
                    }

                    return response;
                }

                // Absolute: "remind me to submit report on 27 June"
                var absoluteMatch = Regex.Match(input, @"remind(?: me)?(?: to)? (.*?) (?:on|at|by) (.*)", RegexOptions.IgnoreCase);
                if (absoluteMatch.Success)
                {
                    string taskText = absoluteMatch.Groups[1].Value.Trim();
                    string timeText = absoluteMatch.Groups[2].Value.Trim();

                    var matchingTask = taskList.FirstOrDefault(t => t.Title.ToLower().Contains(taskText.ToLower()));
//changed                    DateTime? reminderDate = ParseNaturalDate(timeText);
                    var reminderDate = ParseNaturalDate(timeText);

                    if (matchingTask != null && reminderDate.HasValue)
                    {
                        matchingTask.ReminderDate = reminderDate;
                        response.CreatedTask = matchingTask;
                        response.Message = $"Reminder set for \"{matchingTask.Title}\" on {reminderDate.Value.ToShortDateString()}.";
                        ActivityLogger.Log($"Reminder added: \"{matchingTask.Title}\"");
                    }
                    else
                    {
                        response.Message = matchingTask == null
                            ? $"Task \"{taskText}\" not found."
                            : $"Could not understand date: \"{timeText}\".";
                    }

                    return response;
                }
            }

            //Basic task adding dialogue
            if (input.ToLower().Contains("add a task"))
            {
                awaitingTaskTitle = true;
                response.Message = "What is the title of the task you'd like to add?";
                return response;
            }

            if (awaitingTaskTitle)
            {
                pendingTaskTitle = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
                awaitingTaskTitle = false;
                awaitingDescription = true;

                response.Message = $"What should the description be for \"{pendingTaskTitle}\"?";
                return response;
            }

            if (awaitingDescription)
            {
                pendingTaskDescription = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
                awaitingDescription = false;

                var task = new TaskItem
                {
                    Title = pendingTaskTitle,
                    Description = pendingTaskDescription,
                    ReminderDate = null,
                    IsCompleted = false
                };

                pendingTaskTitle = null;
                pendingTaskDescription = null;

                response.CreatedTask = task;
                response.Message = $"Task \"{task.Title}\" with description \"{task.Description}\" added.";
                ActivityLogger.Log($"Task added: \"{task.Title}\"");
                return response;
            }

            //Quiz logic
            if (input.ToLower().Contains("quiz") || input.ToLower().Contains("test"))
            {
                LoadQuizQuestions();
                currentQuizIndex = 0;
                quizScore = 0;
                isInQuiz = true;
                awaitingQuizAnswer = true;

                response.Message = $"Starting quiz!\n\n{FormatQuestion(quizQuestions[currentQuizIndex])}";
                ActivityLogger.Log($"Quiz started added:");
                return response;
            }

            if (isInQuiz && awaitingQuizAnswer)
            {
                var question = quizQuestions[currentQuizIndex];
                string answer = input.Trim().ToUpper();

                bool isCorrect = question.IsTrueFalse
                    ? answer == question.CorrectAnswer.ToUpper()
                    : answer == question.CorrectAnswer.ToUpper().Substring(0, 1);

                string feedback = isCorrect ? "Correct!" : $"Incorrect. The correct answer is {question.CorrectAnswer}.";
                string explanation = $"Explanation: {question.Explanation}";

                if (isCorrect) quizScore++;
                currentQuizIndex++;

                if (currentQuizIndex < quizQuestions.Count)
                {
                    response.Message = $"{feedback}\n{explanation}\n\nNext:\n{FormatQuestion(quizQuestions[currentQuizIndex])}";
                }
                else
                {
                    isInQuiz = false;
                    awaitingQuizAnswer = false;
                    string resultFeedback = quizScore >= 8
                        ? "Great job! You're a cybersecurity pro!"
                        : "Keep learning to stay safe online!";
                    ActivityLogger.Log($"Quiz ended added: \"{quizScore}\"");
                    response.Message = $"{feedback}\n{explanation}\n\nQuiz complete! You scored {quizScore}/{quizQuestions.Count}.\n{resultFeedback}";
                }

                return response;
            }

            // Default fallback
            response.Message = ProcessChat(input);
            return response;
        }

    }
}