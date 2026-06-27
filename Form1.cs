using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CybersecurityAwarenessBot;

namespace CybersecurityChatbotGUI
{
    public partial class Form1 : Form
    {
        // ── Part 1 & 2 — Core services (EXACTLY as your original) ────────
        private ResponseManager _responseManager;
        private AudioManager _audioManager;

        // ── Part 1 & 2 — Conversation memory (EXACTLY as your original) ──
        private string _userName = "";
        private string _favouriteTopic = "";
        private readonly List<string> _topicsDiscussed = new List<string>();
        private readonly Dictionary<string, string> _conversationMemory = new Dictionary<string, string>();

        // ── Part 1 & 2 — State flags (EXACTLY as your original) ──────────
        private int _invalidInputCount = 0;
        private bool _nameEntered = false;
        private bool _topicEntered = false;

        // ── Part 1 & 2 — WhatsApp colours (EXACTLY as your original) ─────
        private readonly Color _userBubbleColor = Color.FromArgb(220, 248, 198);
        private readonly Color _botBubbleColor = Color.White;
        private readonly Color _timeStampColor = Color.FromArgb(150, 150, 150);
        private readonly Color _chatBackgroundColor = Color.FromArgb(236, 229, 221);

        // ── Part 3 — New services ─────────────────────────────────────────
        private TaskManager _taskManager;
        private QuizManager _quizManager;
        private ActivityLogger _activityLogger;

        // ── Part 3 — NLP task confirmation state ─────────────────────────
        private bool _waitingForTaskConfirmNlp = false;
        private string _pendingNlpTaskTitle = "";

        // ─────────────────────────────────────────────────────────────────
        //  CONSTRUCTOR
        // ─────────────────────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();
            _responseManager = new ResponseManager();
            _audioManager = new AudioManager();
            // Part 3 services
            _taskManager = new TaskManager();
            _quizManager = new QuizManager();
            _activityLogger = new ActivityLogger();
        }

        // ─────────────────────────────────────────────────────────────────
        //  FORM LOAD — exactly as Part 2, audio + avatar + ASCII + welcome
        // ─────────────────────────────────────────────────────────────────
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadAvatarImage();
            _audioManager.PlayGreeting();
            AddBotMessage(GetAsciiArt(), Color.Black);
            AddBotMessage("🔒 WELCOME TO THE CYBERSECURITY AWARENESS BOT. YOUR PERSONAL GUIDE TO STAYING SAFE ONLINE! 🔒", Color.Black);
            AddBotMessage("👋 Hello! I'm your Cybersecurity Awareness Bot.How can I help you stay safe online today?But first — may I please have your name? 😊", Color.Black);
        }

        // ─────────────────────────────────────────────────────────────────
        //  AVATAR CLICK — exactly as Part 2
        // ─────────────────────────────────────────────────────────────────
        private void picAvatar_Click(object sender, EventArgs e)
        {
            if (picAvatar.Image == null)
            {
                MessageBox.Show(
                    "No profile picture found.\n\nPlace a file named 'profile.jpg' in the same folder as the application to set a profile picture.",
                    "Profile Picture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var preview = new Form
            {
                Text = "Profile Picture", Size = new Size(300, 340),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.FromArgb(18, 140, 126)
            };
            var big = new PictureBox
            {
                Size = new Size(220, 220), Location = new Point(40, 20),
                SizeMode = PictureBoxSizeMode.StretchImage, Image = picAvatar.Image,
                BackColor = Color.FromArgb(37, 211, 102)
            };
            var title = new Label
            {
                Text = "Cybersecurity Awareness Bot", ForeColor = Color.Black,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(260, 24), Location = new Point(20, 255),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var subtitle = new Label
            {
                Text = "online", ForeColor = Color.FromArgb(200, 255, 200),
                Font = new Font("Segoe UI", 9F), Size = new Size(260, 20),
                Location = new Point(20, 280), TextAlign = ContentAlignment.MiddleCenter
            };
            preview.Controls.Add(big); preview.Controls.Add(title); preview.Controls.Add(subtitle);
            preview.ShowDialog(this);
        }

        // ─────────────────────────────────────────────────────────────────
        //  SEND MESSAGE
        // ─────────────────────────────────────────────────────────────────
        private void btnSend_Click(object sender, EventArgs e) => SendMessage();

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            SendMessage();
        }

        private void SendMessage()
        {
            string userInput = txtInput.Text.Trim();
            txtInput.Clear();

            // 1) Empty input — exactly as Part 2
            if (string.IsNullOrWhiteSpace(userInput))
            {
                _invalidInputCount++;
                AddBotMessage("⚠️ Please type something!", Color.Black);
                if (_invalidInputCount >= 3)
                {
                    AddBotMessage("💡 Try asking about passwords, phishing, or safe browsing!", Color.Black);
                    _invalidInputCount = 0;
                }
                return;
            }

            _invalidInputCount = 0;
            AddUserMessage(userInput);
            string lower = userInput.ToLower().Trim();

            // 2) Ask for name — exactly as Part 2
            if (!_nameEntered)
            {
                _userName = CapitalizeName(userInput);
                _nameEntered = true;
                lblStatus.Text = "🟢 " + _userName + " is chatting...";
                AddBotMessage(
                    "Hello " + _userName + "! Welcome! 👋\n\n" +
                    "What is your favourite cybersecurity topic?\n" +
                    "(e.g. phishing, passwords, safe browsing)", Color.Black);
                return;
            }

            // 3) Ask for topic — exactly as Part 2 + Part 3 commands added
            if (!_topicEntered)
            {
                _favouriteTopic = userInput;
                _topicEntered = true;
                _activityLogger.Log("Session started. Favourite topic: " + _favouriteTopic);
                AddBotMessage(
                    "Great! I'll remember that you're interested in " + _favouriteTopic + ". 🔒\n\n" +
                    "It's a crucial part of staying safe online!\n\n" +
                    "Feel free to ask me anything about cybersecurity!\n\n" +
                    "You can also ask me:\n" +
                    "• 'what have we talked about' — to see our conversation history\n" +
                    "• 'give me a tip' — for safety tips\n" +
                    "• 'give me an example' — for real world examples\n" +
                    "• 'give me a fun fact' — for interesting facts\n\n" +
                    "🆕 NEW FEATURES:\n" +
                    "• 'start quiz' — test your cybersecurity knowledge 🎮\n" +
                    "• 'add task' — add a cybersecurity task 📋\n" +
                    "• 'view tasks' — see your task list\n" +
                    "• 'complete task 1' — mark task as done\n" +
                    "• 'delete task 1' — remove a task\n" +
                    "• 'show activity log' — see recent actions",
                    Color.Black);
                return;
            }

            // 4) Exit commands — exactly as Part 2
            if (IsExitCommand(lower))
            {
                _activityLogger.Log("Session ended by user.");
                AddBotMessage(
                    "Goodbye " + _userName + "! Stay safe online! 🔒\n" +
                    "Remember: keep learning about " + _favouriteTopic + "!", Color.Black);
                if (_topicsDiscussed.Count > 0)
                {
                    string summary = "Here is a summary of what we covered today:\n\n";
                    foreach (string topic in _topicsDiscussed)
                        summary += "✅ " + topic.ToUpper() + "\n";
                    AddBotMessage(summary, Color.Black);
                }
                btnSend.Enabled = false;
                txtInput.Enabled = false;
                lblStatus.Text = "⚫ offline";
                return;
            }

            // ── PART 3: Activity log ──────────────────────────────────────
            if (lower.Contains("show activity log") || lower.Contains("activity log") ||
                lower.Contains("what have you done for me") || lower.Contains("what have you done"))
            {
                AddBotMessage(_activityLogger.GetLog(), Color.Black);
                return;
            }

            // 5) Memory recall — exactly as Part 2
            if (IsMemoryQuestion(lower))
            {
                AddBotMessage(HandleMemoryRecall(), Color.Black);
                return;
            }

            // ── PART 3: Quiz is active — pass answer to quiz ──────────────
            if (_quizManager.IsQuizActive)
            {
                if (lower == "stop quiz")
                {
                    string stopMsg = _quizManager.StopQuiz();
                    _activityLogger.Log("Quiz stopped by user.");
                    AddBotMessage(stopMsg, Color.Black);
                    return;
                }
                string quizResponse = _quizManager.AnswerQuestion(userInput);
                _activityLogger.Log("Quiz answer submitted: " + userInput);
                AddBotMessage(quizResponse, Color.Black);
                return;
            }

            // ── PART 3: NLP task confirmation ─────────────────────────────
            if (_waitingForTaskConfirmNlp)
            {
                _waitingForTaskConfirmNlp = false;
                if (lower.Contains("yes") || lower.Contains("sure") || lower.Contains("ok") || lower.Contains("yeah"))
                    OpenTaskForm(_pendingNlpTaskTitle);
                else
                    AddBotMessage("No problem! Let me know if you change your mind. 😊", Color.Black);
                _pendingNlpTaskTitle = "";
                return;
            }

            // ── PART 3: Task commands ─────────────────────────────────────
            if (lower.Contains("add task") || lower.Contains("new task") || lower.Contains("create task"))
            { OpenTaskForm(); return; }

            if (lower.Contains("view task") || lower.Contains("show task") || lower.Contains("my task") || lower.Contains("list task"))
            { AddBotMessage(_taskManager.GetAllTasks(), Color.Black); _activityLogger.Log("User viewed task list."); return; }

            if (lower.Contains("complete task") || lower.Contains("mark task") || lower.Contains("finish task") || lower.Contains("done task"))
            {
                int id = ExtractNumber(lower);
                if (id > 0) { string r = _taskManager.CompleteTask(id); _activityLogger.Log("Task completed: ID " + id); AddBotMessage(r, Color.Black); }
                else AddBotMessage("Please include the task ID.\nExample: 'complete task 1'\n\n" + _taskManager.GetAllTasks(), Color.Black);
                return;
            }

            if (lower.Contains("delete task") || lower.Contains("remove task"))
            {
                int id = ExtractNumber(lower);
                if (id > 0) { string r = _taskManager.DeleteTask(id); _activityLogger.Log("Task deleted: ID " + id); AddBotMessage(r, Color.Black); }
                else AddBotMessage("Please include the task ID.\nExample: 'delete task 2'\n\n" + _taskManager.GetAllTasks(), Color.Black);
                return;
            }

            // ── PART 3: Quiz start ────────────────────────────────────────
            if (lower.Contains("start quiz") || lower.Contains("quiz") || lower.Contains("test me") || lower.Contains("play game"))
            {
                string quizStart = _quizManager.StartQuiz();
                _activityLogger.Log("Quiz started.");
                AddBotMessage(quizStart, Color.Black);
                return;
            }

            // 6) Sentiment detection — exactly as Part 2
            string sentimentResponse = DetectSentiment(lower);
            if (sentimentResponse != null)
            {
                AddBotMessage(sentimentResponse, Color.Black);
                string sentimentTopic = DetectTopicFromInput(lower) ?? _favouriteTopic;
                if (!string.IsNullOrEmpty(sentimentTopic))
                {
                    string tip = _responseManager.GetResponse("give me a tip " + sentimentTopic);
                    tip = tip.Replace("{user}", _userName);
                    AddBotMessage("Here is something that might help you " + _userName + ":\n\n" + tip, Color.Black);
                }
                AddBotMessage("Remember " + _userName + ", you are doing great by learning about cybersecurity! Feel free to keep asking me anything! 💪", Color.Black);
                return;
            }

            // ── PART 3: NLP — detect task/reminder intent ────────────────
            string nlpResult = TryHandleNlpIntent(lower, userInput);
            if (nlpResult != null)
            {
                AddBotMessage(nlpResult, Color.Black);
                _activityLogger.Log("NLP command recognised: " + userInput);
                return;
            }

            // 7) Main response — exactly as Part 2
            string response = _responseManager.GetResponse(userInput);
            response = response.Replace("{user}", _userName);

            string detectedTopic = DetectTopicFromInput(lower);
            if (detectedTopic != null)
            {
                if (!_topicsDiscussed.Contains(detectedTopic))
                    _topicsDiscussed.Add(detectedTopic);
                _conversationMemory[detectedTopic] = userInput;
                _activityLogger.Log("Topic discussed: " + detectedTopic);
            }

            if (!string.IsNullOrEmpty(_favouriteTopic) &&
                response.Contains("cybersecurity") &&
                !lower.Contains(_favouriteTopic.ToLower()))
            {
                response += "\n\n💡 As someone interested in " + _favouriteTopic +
                            ", you might want to explore that topic further too!";
            }

            AddBotMessage(response, Color.Black);
        }

        // ─────────────────────────────────────────────────────────────────
        //  PART 3: NLP intent detection
        // ─────────────────────────────────────────────────────────────────
        private string TryHandleNlpIntent(string lower, string original)
        {
            bool hasRemind = lower.Contains("remind") || lower.Contains("reminder") || lower.Contains("don't forget") || lower.Contains("remember to");
            bool hasTask = lower.Contains("task") || lower.Contains("to-do") || lower.Contains("add") || lower.Contains("set") || lower.Contains("schedule");
            bool hasPassword = lower.Contains("password") || lower.Contains("2fa") || lower.Contains("two factor") || lower.Contains("two-factor");
            bool hasSecurity = lower.Contains("privacy") || lower.Contains("antivirus") || lower.Contains("update") || lower.Contains("backup") || lower.Contains("firewall") || lower.Contains("vpn");

            if (hasRemind && (hasPassword || hasSecurity || hasTask))
            {
                string title = InferTaskTitle(lower);
                _waitingForTaskConfirmNlp = true;
                _pendingNlpTaskTitle = title;
                return $"📋 I can add '{title}' as a task with a reminder for you.\nWould you like me to do that? (yes / no)";
            }
            if ((hasTask || lower.Contains("need to") || lower.Contains("i should")) && (hasPassword || hasSecurity))
            {
                string title = InferTaskTitle(lower);
                _waitingForTaskConfirmNlp = true;
                _pendingNlpTaskTitle = title;
                return $"📋 Would you like me to add '{title}' as a cybersecurity task? (yes / no)";
            }
            return null;
        }

        private string InferTaskTitle(string lower)
        {
            if (lower.Contains("password")) return "Update my password";
            if (lower.Contains("2fa") || lower.Contains("two factor") || lower.Contains("two-factor")) return "Enable two-factor authentication";
            if (lower.Contains("privacy")) return "Review account privacy settings";
            if (lower.Contains("antivirus")) return "Run antivirus scan";
            if (lower.Contains("update")) return "Install software updates";
            if (lower.Contains("backup")) return "Back up important data";
            if (lower.Contains("firewall")) return "Check firewall settings";
            if (lower.Contains("vpn")) return "Set up VPN";
            return "Complete cybersecurity task";
        }

        // ─────────────────────────────────────────────────────────────────
        //  PART 3: Task form popup
        // ─────────────────────────────────────────────────────────────────
        private void OpenTaskForm(string prefilledTitle = "")
        {
            using (var form = new TaskForm(prefilledTitle))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    string result = _taskManager.AddTask(form.TaskTitle, form.TaskDescription, form.ReminderDate);
                    _activityLogger.Log("Task added: '" + form.TaskTitle + "'" +
                        (string.IsNullOrEmpty(form.ReminderDate) ? "" : " (Reminder: " + form.ReminderDate + ")"));
                    AddBotMessage(result, Color.Black);
                    AddBotMessage("Type 'view tasks' to see all tasks.\nType 'complete task [ID]' or 'delete task [ID]' to manage them.", Color.Black);
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────
        //  CHAT UI — exactly as Part 2
        // ─────────────────────────────────────────────────────────────────
        private void AddUserMessage(string message)
        {
            string time = DateTime.Now.ToString("HH:mm");
            rtbChat.AppendText("\n");
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.SelectionAlignment = HorizontalAlignment.Right;
            rtbChat.SelectionBackColor = _chatBackgroundColor;
            rtbChat.SelectionColor = _chatBackgroundColor;
            rtbChat.AppendText("          ");
            rtbChat.SelectionAlignment = HorizontalAlignment.Right;
            rtbChat.SelectionBackColor = _userBubbleColor;
            rtbChat.SelectionColor = Color.Black;
            rtbChat.SelectionFont = new Font("Segoe UI", 10F);
            rtbChat.AppendText("  " + message + "  ");
            rtbChat.AppendText("\n");
            rtbChat.SelectionAlignment = HorizontalAlignment.Right;
            rtbChat.SelectionBackColor = _chatBackgroundColor;
            rtbChat.SelectionColor = _timeStampColor;
            rtbChat.SelectionFont = new Font("Segoe UI", 8F);
            rtbChat.AppendText(time + " ✓✓\n");
            rtbChat.SelectionAlignment = HorizontalAlignment.Left;
            rtbChat.SelectionBackColor = _chatBackgroundColor;
            rtbChat.ScrollToCaret();
        }

        private void AddBotMessage(string message, Color textColor)
        {
            string time = DateTime.Now.ToString("HH:mm");
            rtbChat.AppendText("\n");
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            rtbChat.SelectionAlignment = HorizontalAlignment.Left;
            rtbChat.SelectionBackColor = _botBubbleColor;
            rtbChat.SelectionColor = textColor;
            rtbChat.SelectionFont = new Font("Courier New", 9F, FontStyle.Bold);
            rtbChat.AppendText("  " + message + "  ");
            rtbChat.AppendText("\n");
            rtbChat.SelectionAlignment = HorizontalAlignment.Left;
            rtbChat.SelectionBackColor = _chatBackgroundColor;
            rtbChat.SelectionColor = _timeStampColor;
            rtbChat.SelectionFont = new Font("Segoe UI", 8F);
            rtbChat.AppendText(time + "\n");
            rtbChat.SelectionAlignment = HorizontalAlignment.Left;
            rtbChat.SelectionBackColor = _chatBackgroundColor;
            rtbChat.ScrollToCaret();
        }

        // ─────────────────────────────────────────────────────────────────
        //  LOGIC HELPERS — exactly as Part 2
        // ─────────────────────────────────────────────────────────────────
        private bool IsExitCommand(string lowerInput) =>
            lowerInput == "exit" || lowerInput == "quit" || lowerInput == "bye" || lowerInput == "goodbye";

        private string DetectSentiment(string input)
        {
            if (ContainsAny(input, "worried", "scary", "scared", "afraid", "nervous", "anxious", "unsafe", "not safe"))
                return "💙 It is completely understandable to feel worried " + _userName + "!\n\n" +
                       "The fact that you are here learning about it means you are already taking the RIGHT steps to protect yourself!\n\n" +
                       "Let me help ease your worries with some helpful information!";
            if (ContainsAny(input, "frustrated", "annoyed", "angry", "useless", "not helping", "dont get it", "don't get it", "makes no sense"))
                return "💙 I am really sorry to hear you are feeling frustrated " + _userName + "!\n\n" +
                       "Cybersecurity topics can be tricky at first — even professionals find some concepts difficult!\n\nPlease do not give up — you are doing better than you think! 😊";
            if (ContainsAny(input, "confused", "dont understand", "don't understand", "lost", "no idea", "not sure", "unclear"))
                return "💙 No worries at all " + _userName + "!\n\n" +
                       "Confusion just means we need to explain it differently.\n\nAsk me to explain anything step by step and I will make it clearer!";
            if (ContainsAny(input, "curious", "interesting", "want to know", "want to learn"))
                return "🌟 I love your curiosity " + _userName + "!\n\n" +
                       "That is exactly the mindset that keeps people safe online!\n\nLet me feed that curiosity with some great information!";
            if (ContainsAny(input, "overwhelmed", "too much", "complicated", "difficult", "lot to learn"))
                return "💙 I completely understand " + _userName + "!\n\n" +
                       "You do not need to learn everything at once.\n\nStart with one small habit like using stronger passwords and build from there. Small steps lead to big protection! 🔒";
            if (ContainsAny(input, "happy", "excited", "great", "awesome", "love this", "amazing"))
                return "🎉 That is wonderful to hear " + _userName + "!\n\n" +
                       "People who enjoy learning about cybersecurity are the ones who stay safest online.\n\nLet us keep that momentum going!";
            if (ContainsAny(input, "thank", "thanks", "appreciate", "helpful"))
                return "😊 You are so welcome " + _userName + "!\n\n" +
                       "Your safety matters and you deserve to feel confident in the digital world.\n\nKeep asking questions — there is always more to learn!";
            return null;
        }

        private bool ContainsAny(string input, params string[] keywords)
        {
            foreach (var keyword in keywords)
                if (input.Contains(keyword)) return true;
            return false;
        }

        private bool IsMemoryQuestion(string input) =>
            input.Contains("remember") || input.Contains("recall") ||
            input.Contains("what did i ask") || input.Contains("what have we talked") ||
            input.Contains("what did we talk") || input.Contains("what have we discussed") ||
            input.Contains("what topics") || input.Contains("our conversation") ||
            input.Contains("history") || input.Contains("previously") ||
            input.Contains("what did we cover") || input.Contains("earlier");

        private string HandleMemoryRecall()
        {
            if (_topicsDiscussed.Count == 0)
                return "We have not discussed any cybersecurity topics yet " + _userName +
                       "! Ask me about phishing, passwords or safe browsing!";

            string memory = "🧠 Here is what I remember about our conversation " + _userName + ":\n\n" +
                            "👤 Your name: " + _userName + "\n" +
                            "⭐ Your favourite topic: " + _favouriteTopic + "\n\n" +
                            "📚 Topics we have discussed so far:\n";
            foreach (string topic in _topicsDiscussed)
            {
                memory += "\n   ✅ " + topic.ToUpper() + "\n";
                if (_conversationMemory.ContainsKey(topic))
                    memory += "      You asked: '" + _conversationMemory[topic] + "'\n";
            }
            memory += "\n💡 As someone interested in " + _favouriteTopic +
                      ", you might also want to explore the other topics!\nJust type any topic name to continue learning!";
            return memory;
        }

        private string DetectTopicFromInput(string input)
        {
            if (input.Contains("phish") || input.Contains("scam") || input.Contains("spam")) return "phishing";
            if (input.Contains("password") || input.Contains("passphrase") || input.Contains("2fa")) return "password";
            if (input.Contains("browsing") || input.Contains("vpn") || input.Contains("https") ||
                input.Contains("wifi") || input.Contains("privacy")) return "safe browsing";
            return null;
        }

        private string CapitalizeName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (name.Length == 1) return name.ToUpper();
            return char.ToUpper(name[0]) + name.Substring(1).ToLower();
        }

        private int ExtractNumber(string input)
        {
            foreach (var word in input.Split(' '))
                if (int.TryParse(word, out int n)) return n;
            return -1;
        }

        private string GetAsciiArt()
        {
            return
                "==========================================\n" +
                "\n" +
                "  #####  #   #  ####  ####  ####\n" +
                "  #       # #   #  #  #     #  #\n" +
                "  #        #    ####  ###   ####\n" +
                "  #        #    #  #  #     #  #\n" +
                "  #####    #    #  #  ####  #  #\n" +
                "\n" +
                "  ####   ###  #####\n" +
                "  #  #  #  #  #\n" +
                "  ####  #  #  #\n" +
                "  #  #  #  #  #\n" +
                "  ####   ###   #\n" +
                "\n" +
                "==========================================\n" +
                "   CYBERSECURITY AWARENESS BOT\n" +
                "==========================================";
        }
    }
}
