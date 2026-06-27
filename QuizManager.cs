using System;
using System.Collections.Generic;
using System.Text;

namespace CybersecurityChatbotGUI
{
    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
        public bool IsMultipleChoice => Options != null && Options.Count > 0;

        public QuizQuestion(string question, List<string> options, string correctAnswer, string explanation)
        { Question = question; Options = options; CorrectAnswer = correctAnswer; Explanation = explanation; }
    }

    public class QuizManager
    {
        private List<QuizQuestion> _allQuestions;
        private List<QuizQuestion> _sessionQuestions;
        private int _currentIndex, _score;
        private bool _quizActive;

        public bool IsQuizActive => _quizActive;
        public int Score => _score;
        public int TotalQuestions => _sessionQuestions?.Count ?? 0;

        public QuizManager() { BuildQuestions(); }

        public string StartQuiz()
        {
            _sessionQuestions = Shuffle(_allQuestions);
            if (_sessionQuestions.Count > 10) _sessionQuestions = _sessionQuestions.GetRange(0, 10);
            _currentIndex = 0; _score = 0; _quizActive = true;
            return "🎮 CYBERSECURITY QUIZ STARTED!\n\n" +
                   $"You will answer {_sessionQuestions.Count} questions.\n" +
                   "Type A / B / C / D for multiple choice, or True / False for T/F questions.\n" +
                   "Type 'stop quiz' at any time to quit.\n\n" + GetCurrentQuestion();
        }

        public string AnswerQuestion(string userAnswer)
        {
            if (!_quizActive) return "No quiz active. Type 'start quiz' to begin!";
            if (_currentIndex >= _sessionQuestions.Count) return FinishQuiz();

            var q = _sessionQuestions[_currentIndex];
            string answer = userAnswer.Trim().ToUpper();
            if (answer == "T") answer = "TRUE";
            if (answer == "F") answer = "FALSE";

            bool correct = answer.Equals(q.CorrectAnswer.ToUpper(), StringComparison.OrdinalIgnoreCase);
            var sb = new StringBuilder();
            if (correct) { _score++; sb.AppendLine("✅ CORRECT! Well done!"); }
            else sb.AppendLine($"❌ INCORRECT. The correct answer was: {q.CorrectAnswer}");
            sb.AppendLine($"\n💡 Explanation: {q.Explanation}");
            sb.AppendLine($"\n📊 Score so far: {_score}/{_currentIndex + 1}");
            _currentIndex++;
            if (_currentIndex >= _sessionQuestions.Count)
                sb.AppendLine("\n" + FinishQuiz());
            else { sb.AppendLine("\n─────────────────────────────"); sb.AppendLine(GetCurrentQuestion()); }
            return sb.ToString();
        }

        public string GetCurrentQuestion()
        {
            if (_currentIndex >= _sessionQuestions.Count) return FinishQuiz();
            var q = _sessionQuestions[_currentIndex];
            var sb = new StringBuilder();
            sb.AppendLine($"❓ Question {_currentIndex + 1} of {_sessionQuestions.Count}:");
            sb.AppendLine(q.Question);
            if (q.IsMultipleChoice) foreach (var opt in q.Options) sb.AppendLine(opt);
            else sb.AppendLine("Options: True / False");
            return sb.ToString();
        }

        public string StopQuiz()
        {
            _quizActive = false;
            return $"Quiz stopped. You answered {_currentIndex} question(s) and scored {_score} point(s).";
        }

        private string FinishQuiz()
        {
            _quizActive = false;
            int total = _sessionQuestions.Count;
            string rating;
            if (_score == total) rating = "🏆 PERFECT SCORE! You are a cybersecurity expert!";
            else if (_score >= total * 0.8) rating = "🌟 Excellent! You really know your stuff!";
            else if (_score >= total * 0.6) rating = "👍 Good job! Keep on learning!";
            else if (_score >= total * 0.4) rating = "💪 Not bad — a bit more studying will help!";
            else rating = "📚 Keep practising — cybersecurity knowledge is very important!";
            return $"🏁 QUIZ COMPLETE!\n\nFinal Score: {_score} / {total}\n{rating}\n\nType 'start quiz' to play again!";
        }

        private List<QuizQuestion> Shuffle(List<QuizQuestion> list)
        {
            var copy = new List<QuizQuestion>(list);
            var rng = new Random();
            for (int i = copy.Count - 1; i > 0; i--)
            { int j = rng.Next(i + 1); var tmp = copy[i]; copy[i] = copy[j]; copy[j] = tmp; }
            return copy;
        }

        private void BuildQuestions()
        {
            _allQuestions = new List<QuizQuestion>
            {
                new QuizQuestion("What is phishing?",
                    new List<string> { "A) A fishing sport", "B) A cyberattack using fake emails to steal information", "C) A software update", "D) A secure login method" },
                    "B", "Phishing is a cyberattack where criminals send fake emails pretending to be legitimate organisations to trick you into giving up personal information."),

                new QuizQuestion("Which of the following is the STRONGEST password?",
                    new List<string> { "A) password123", "B) Maria2001", "C) Tr0ub4dor&3!xQ", "D) 123456" },
                    "C", "A strong password uses a mix of uppercase, lowercase, numbers and special characters and is at least 12 characters long."),

                new QuizQuestion("What does HTTPS stand for?",
                    new List<string> { "A) HyperText Transfer Protocol Secure", "B) High Transfer Technology Protocol System", "C) Hyperlink Text Processing Standard", "D) Home Transfer Protocol Service" },
                    "A", "HTTPS stands for HyperText Transfer Protocol Secure. The S means the connection between your browser and the website is encrypted."),

                new QuizQuestion("What is Two-Factor Authentication (2FA)?",
                    new List<string> { "A) Logging in with two different passwords", "B) A second verification step added after your password", "C) Using two different browsers", "D) Sharing your login with a trusted person" },
                    "B", "2FA adds an extra layer of security. Even if your password is stolen, 2FA stops the attacker by requiring a second verification step."),

                new QuizQuestion("What is ransomware?",
                    new List<string> { "A) Software that speeds up your computer", "B) A type of antivirus program", "C) Malware that locks your files and demands payment", "D) A secure file backup tool" },
                    "C", "Ransomware is malicious software that encrypts your files. Attackers then demand a ransom to provide the decryption key."),

                new QuizQuestion("Which is a sign that an email might be phishing?",
                    new List<string> { "A) It comes from your bank's official domain", "B) It uses your full name and account number", "C) It says 'Dear Customer' and asks you to click a link urgently", "D) It has your recent transaction history" },
                    "C", "Generic greetings like 'Dear Customer', urgent language and suspicious links are classic phishing red flags."),

                new QuizQuestion("What is a VPN used for?",
                    new List<string> { "A) To speed up your internet connection", "B) To encrypt your internet traffic and hide your IP address", "C) To block all ads on websites", "D) To store your passwords securely" },
                    "B", "A VPN encrypts your internet connection and masks your IP address, protecting your privacy especially on public Wi-Fi."),

                new QuizQuestion("How often should you update your passwords?",
                    new List<string> { "A) Never", "B) Every 10 years", "C) Only when you forget them", "D) Regularly, especially after a data breach or suspicious activity" },
                    "D", "You should update passwords regularly and immediately after a breach or suspicious activity."),

                new QuizQuestion("What does malware mean?",
                    new List<string> { "A) A type of computer hardware", "B) Malicious software designed to damage or gain unauthorised access", "C) A strong password", "D) A secure email protocol" },
                    "B", "Malware stands for malicious software. It includes viruses, ransomware, spyware and trojans designed to harm your device or steal your data."),

                new QuizQuestion("What is social engineering in cybersecurity?",
                    new List<string> { "A) Building social media apps", "B) Manipulating people into revealing confidential information", "C) Engineering better social networks", "D) A type of firewall" },
                    "B", "Social engineering manipulates human psychology — trust, fear or urgency — to trick people into revealing passwords or sensitive information."),

                new QuizQuestion("True or False: Using the same password for multiple accounts is safe as long as it is a strong password.",
                    null, "False", "Using the same password everywhere is dangerous. If one account is compromised, all accounts using that password are at risk."),

                new QuizQuestion("True or False: A padlock icon in your browser means the website is 100% safe and trustworthy.",
                    null, "False", "The padlock only means the connection is encrypted. Phishing sites can also use HTTPS and show a padlock icon."),

                new QuizQuestion("True or False: Public Wi-Fi networks are generally safe to use for online banking.",
                    null, "False", "Public Wi-Fi is often unsecured, making it easy for attackers to intercept your data. Never do banking on public Wi-Fi without a VPN."),

                new QuizQuestion("True or False: Software updates often include important security patches that fix vulnerabilities.",
                    null, "True", "Security patches fix known vulnerabilities. Delaying updates leaves your system exposed to attackers."),

                new QuizQuestion("True or False: Antivirus software alone is enough to protect you from all cybersecurity threats.",
                    null, "False", "Antivirus is important but not sufficient. Good cybersecurity requires strong passwords, 2FA, updates, careful browsing and general awareness.")
            };
        }
    }
}
