using System;
using System.Drawing;
using System.Windows.Forms;
namespace ChatBot
{
    public partial class ChatBot : Form
    {
        private readonly BotService botService; // Service to handle bot responses
        private readonly Label typingLabel; // Label to indicate bot is typing
        private readonly Timer typingTimer; // Timer to animate typing indicator
        private int dotCount = 0; // Counter for typing animation dots

        public ChatBot()
        {
            InitializeComponent();

            // Initialize typing label with default properties
            typingLabel = new Label
            {
                Text = string.Empty, // Use string.Empty for better readability
                ForeColor = Color.Gray, // Gray color for typing indicator
                Font = new Font("Segoe UI", 10, FontStyle.Italic), // Italic font style
                Visible = false // Initially hidden
            };

            // Initialize typing timer with interval for animation
            typingTimer = new Timer
            {
                Interval = 400 // 400ms interval for typing animation
            };
            typingTimer.Tick += TypingTimer_Tick; // Attach event handler for timer tick

            // Add typing label to chat panel and ensure proper positioning
            chatPanel.Controls.Remove(typingLabel);
            chatPanel.Controls.Add(typingLabel);

            // Scroll to ensure typing label is visible
            chatPanel.ScrollControlIntoView(typingLabel);

            // Initialize bot service for handling bot responses
            botService = new BotService();

            // Attach event handler for input box key down event
            this.inputBox.KeyDown += new KeyEventHandler(this.inputBox_KeyDown);
        }

        // Handles Enter key press in input box
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift) // Check for Enter key without Shift
            {
                e.SuppressKeyPress = true; // Prevent default Enter key behavior
                sendButton.PerformClick(); // Trigger send button click
            }
        }

        // Updates typing label text to simulate typing animation
        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            dotCount = (dotCount + 1) % 4; // Cycle through 0 to 3 dots
            typingLabel.Text = "Bot is typing" + new string('.', dotCount); // Update label text
        }

        // Adds a message bubble to the chat panel
        private void AddMessageBubble(string message, bool isUser)
        {
            // Create container panel for the message bubble
            Panel bubblePanel = new Panel
            {
                AutoSize = true, // Automatically adjust size
                Width = chatPanel.Width - 20, // Adjust width to fit chat panel
                Padding = new Padding(10), // Padding around the bubble
                Margin = new Padding(5) // Margin between bubbles
            };

            // Create label for the message bubble
            Label bubble = new Label
            {
                AutoSize = true, // Automatically adjust size
                MaximumSize = new Size(chatPanel.Width - 100, 0), // Max width for wrapping
                MinimumSize = new Size(50, 0), // Minimum width
                Padding = new Padding(10), // Padding inside the bubble
                Font = new Font("Segoe UI", 10), // Font style
                Text = message // Set message text
            };

            if (isUser)
            {
                // Style for user messages
                bubble.BackColor = Color.FromArgb(0, 120, 215); // Blue background
                bubble.ForeColor = Color.White; // White text
                bubble.TextAlign = ContentAlignment.MiddleRight; // Right-aligned text
                bubble.Margin = new Padding(10, 5, 10, 5); // Margin for user bubbles
            }
            else
            {
                // Style for bot messages
                bubble.BackColor = Color.FromArgb(50, 50, 50); // Dark gray background
                bubble.ForeColor = Color.White; // White text
                bubble.TextAlign = ContentAlignment.MiddleLeft; // Left-aligned text
                bubble.Margin = new Padding(10, 5, 10, 5); // Margin for bot bubbles
            }

            // Add bubble to the container panel
            bubblePanel.Controls.Add(bubble);

            // Add container panel to chat panel
            chatPanel.Controls.Add(bubblePanel);

            // Scroll to ensure the new bubble is visible
            chatPanel.ScrollControlIntoView(bubblePanel);
        }

        // Animates the bot's response character by character
        private void AnimateBotResponse(string botResponse)
        {
            // Create label for the animated bot response
            Label messageLabel = new Label
            {
                Text = "", // Start with empty text
                AutoSize = true, // Automatically adjust size
                MaximumSize = new Size(800, 0), // Max width for wrapping
                Padding = new Padding(10), // Padding inside the label
                Font = new Font("Segoe UI", 10F), // Font style
                BackColor = Color.FromArgb(50, 50, 50), // Dark gray background
                ForeColor = Color.White, // White text
                TextAlign = ContentAlignment.MiddleLeft, // Left-aligned text
                Margin = new Padding(10, 5, 10, 5) // Margin for the label
            };

            // Add label to chat panel
            this.chatPanel.Controls.Add(messageLabel);
            this.chatPanel.ScrollControlIntoView(messageLabel); // Scroll to make it visible

            // Timer for animating the response
            Timer typingTimer = new Timer
            {
                Interval = 30 // 30ms interval for character animation
            };
            int index = 0; // Index to track current character

            // Event handler for timer tick
            typingTimer.Tick += (sender, e) =>
            {
                if (index < botResponse.Length) // Check if more characters remain
                {
                    sendButton.Enabled = false; // Disable send button during animation
                    messageLabel.Text += botResponse[index]; // Append next character
                    index++; // Move to next character
                }
                else
                {
                    typingTimer.Stop(); // Stop timer when animation is complete
                    sendButton.Enabled = true; // Re-enable send button
                }
            };

            typingTimer.Start(); // Start the animation timer
        }

        // Handles send button click event
        private async void sendButton_Click(object sender, EventArgs e)
        {
            string userMessage = inputBox.Text.Trim(); // Get user input and trim whitespace
            if (!string.IsNullOrEmpty(userMessage)) // Check if input is not empty
            {
                // Add user message to chat panel
                AddMessageBubble(userMessage, true);
                inputBox.Clear(); // Clear input box

                // Ensure typing label is not already added
                if (chatPanel.Controls.Contains(typingLabel))
                {
                    chatPanel.Controls.Remove(typingLabel);
                }

                // Add typing label at the bottom of chat panel
                chatPanel.Controls.Add(typingLabel);
                typingLabel.Visible = true; // Show typing label
                typingTimer.Start(); // Start typing animation

                // Scroll to ensure typing label is visible
                chatPanel.ScrollControlIntoView(typingLabel);

                try
                {
                    // Get bot response asynchronously
                    string botResponse = await botService.GetBotResponseAsync(userMessage);

                    // Hide typing indicator
                    typingTimer.Stop(); // Stop typing animation
                    typingLabel.Visible = false; // Hide typing label
                    typingLabel.Text = ""; // Clear typing label text
                    chatPanel.Controls.Remove(typingLabel); // Remove typing label from UI

                    // Add animated bot response to chat panel
                    AnimateBotResponse(botResponse);
                }
                catch (Exception ex)
                {
                    // Handle errors during bot response retrieval
                    typingTimer.Stop(); // Stop typing animation
                    typingLabel.Visible = false; // Hide typing label
                    typingLabel.Text = ""; // Clear typing label text
                    chatPanel.Controls.Remove(typingLabel); // Remove typing label from UI

                    // Add error message to chat panel
                    AddMessageBubble("Error: " + ex.Message, false);
                }
            }
        }
    }
}
