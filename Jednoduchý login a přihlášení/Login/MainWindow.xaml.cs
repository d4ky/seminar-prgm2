using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Login
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
            CodeBox1.Focus();
        }

        private void InitializeDatabase()
        {
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }
        }

        private string loggedUsername = "";
        private string loggedPassword = "";
        private bool loggedIn = false;

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsername.Text;
            string password = LoginPassword.Password;

            bool failed = false;
            if (username == "")
            {
                failed = true;
                LoginUsernameMessage.Text = "This field can not be empty!";
                LoginUsernameMessage.Visibility = Visibility.Visible;
            }
            if (password == "")
            {
                failed = true;
                LoginPasswordMessage.Text = "This field can not be empty!";
                LoginPasswordMessage.Visibility = Visibility.Visible;
            }
            if (failed)
            {
                return;
            }

            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                LoginUsernameMessage.Text = "Invalid credentials!";
                LoginUsernameMessage.Visibility = Visibility.Visible;
                return;
            }

            if (user.LockoutEnd > DateTime.UtcNow)
            {
                MessageBox.Show("Too many attempts. Your account is locked!");
                return;
            }

            string pepperedPassword = password + SecurityConfig.Pepper;
            if (!BCrypt.Net.BCrypt.Verify(pepperedPassword, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddSeconds(30);
                }
                db.SaveChanges();
                LoginUsernameMessage.Text = "Invalid credentials!";
                LoginUsernameMessage.Visibility = Visibility.Visible;
                return;
            }
            if (!user.IsEmailVerified)
            {
                MessageBox.Show("Please verify your email before logging in!");
                return;
            }
            user.FailedLoginAttempts = 0;
            db.SaveChanges();
            ResetLoginTab();

            loggedUsername = username;
            loggedPassword = password;
            loggedIn = true;

            string ImagePath = user.ProfileImagePath;
            BitmapImage bitmapImage = new BitmapImage();

            try
            {
                
                using (var stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }
                ProfilePictureBrush.ImageSource = bitmapImage;
            }
            catch
            {

            }

            
            AccountUsername.Text = loggedUsername;
            AccountAdditionalInfo.Text = user.AdditionalInfo;

            LoginTab.Visibility = Visibility.Collapsed;
            RegisterTab.Visibility = Visibility.Collapsed;
            AccountTab.Visibility = Visibility.Visible;
            AccountSettingsTab.Visibility = Visibility.Visible;

            tabControl.SelectedItem = AccountTab;
        }

        private string _pendingVerificationUsername = "";
        private string _pendingVerificationEmail = "";

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsername.Text;
            string password = RegisterPassword.Password;
            string repeatPassword = RegisterRepeatPassword.Password;
            string additionalInfo = RegisterAdditionalInfo.Text;
            string email = RegisterEmail.Text;

            bool failed = false;

            if (username == "")
            {
                failed = true;
                RegisterUsernameMessage.Text = "This field can not be empty!";
                RegisterUsernameMessage.Visibility = Visibility.Visible;
            }

            if (password != repeatPassword)
            {
                failed = true;
                RegisterRepeatMessage.Text = "Passwords do not match!";
                RegisterRepeatMessage.Visibility = Visibility.Visible;
            }

            if (password == "")
            {
                failed = true;
                RegisterPasswordMessage.Text = "This field can not be empty!";
                RegisterPasswordMessage.Visibility = Visibility.Visible;
            }
            if (repeatPassword == "")
            {
                failed = true;
                RegisterRepeatMessage.Text = "This field can not be empty!";
                RegisterRepeatMessage.Visibility = Visibility.Visible;
            }
            if (additionalInfo == "")
            {
                failed = true;
                RegisterAdditionalMessage.Text = "This field can not be empty!";
                RegisterAdditionalMessage.Visibility = Visibility.Visible;
            }

            if (email == "")
            {
                failed = true;
                RegisterEmailMessage.Text = "This field can not be empty!";
                RegisterEmailMessage.Visibility = Visibility.Visible;
            }

            if (ImagePreview.Visibility == Visibility.Collapsed)
            {
                failed = true;
                RegisterImageMessage.Text = "Upload a profile picture!";
                RegisterImageMessage.Visibility = Visibility.Visible;
            }

            if (failed)
            {
                return;
            }

            using var db = new AppDbContext();

            if (db.Users.Any(u => u.Username == username))
            {
                RegisterUsernameMessage.Text = "Username already exists!";
                RegisterUsernameMessage.Visibility = Visibility.Visible;
                return;
            }

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, pattern))
            {
                RegisterEmailMessage.Text = "Invalid email format!";
                RegisterEmailMessage.Visibility = Visibility.Visible;
                return;
            }

            if (db.Users.Any(u => u.Email == email))
            {
                RegisterEmailMessage.Text = "Email already registered!";
                RegisterEmailMessage.Visibility = Visibility.Visible;
                return;
            }

            var random = new Random();
            string verificationCode = random.Next(100000, 999999).ToString();
            DateTime expiryTime = DateTime.Now.AddMinutes(5);
            string pepperedPassword = password + SecurityConfig.Pepper;
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(pepperedPassword, workFactor: 12);
            //MessageBox.Show(passwordHash);
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                AdditionalInfo = additionalInfo,
                ProfileImagePath = $"resources/profile_pics/{username}.jpg",
                IsEmailVerified = false,
                VerificationCode = verificationCode,
                VerificationCodeExpiry = expiryTime,
                FailedLoginAttempts = 0
            };
            db.Users.Add(user);
            db.SaveChanges();
            SaveTempImage(ToggleSwitch.IsChecked ?? false, username);
            ResetRegisterTab();

            try
            {
                new EmailService().SendVerificationEmail(email, verificationCode);
                _pendingVerificationEmail = email;
                _pendingVerificationUsername = username;

                LoginTab.Visibility = Visibility.Collapsed;
                RegisterTab.Visibility = Visibility.Collapsed;
                VerificationTab.Visibility = Visibility.Visible;
                tabControl.SelectedItem = VerificationTab;

                loggedUsername = username;
                loggedPassword = password;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send verification email: {ex.Message}");
                db.Users.Remove(user);
                db.SaveChanges();
            }


            

        




            

            //string ImagePath = user.ProfileImagePath;
            //BitmapImage bitmapImage = new BitmapImage();

            //try
            //{

            //    using (var stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            //    {
            //        bitmapImage.BeginInit();
            //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //        bitmapImage.StreamSource = stream;
            //        bitmapImage.EndInit();
            //    }
            //    ProfilePictureBrush.ImageSource = bitmapImage;
            //}
            //catch
            //{

            //}


            //AccountUsername.Text = loggedUsername;
            //AccountAdditionalInfo.Text = user.AdditionalInfo;

            //LoginTab.Visibility = Visibility.Collapsed;
            //RegisterTab.Visibility = Visibility.Collapsed;
            //AccountTab.Visibility = Visibility.Visible;
            //AccountSettingsTab.Visibility = Visibility.Visible;

            //tabControl.SelectedItem = AccountTab;
        }

        private void VerifyCode_Click(object sender, RoutedEventArgs e)
        {
            string enteredCode = GetVerificationCode();
            if (enteredCode.Length != 6)
            {
                VerificationStatus.Text = "Please enter all 6 digits!";
                return;
            }
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u =>
                u.Email == _pendingVerificationEmail &&
                u.Username == _pendingVerificationUsername);

            if (user == null || user.VerificationCodeExpiry < DateTime.Now)
            {
                VerificationStatus.Text = "Invalid or expired code!";
                return;
            }

            if (enteredCode != user.VerificationCode)
            {
                VerificationStatus.Text = "Invalid verification code!";
                return;
            }

            user.IsEmailVerified = true;
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
            db.SaveChanges();


            loggedIn = true;

            string ImagePath = user.ProfileImagePath;
            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                using (var stream = new FileStream(ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }
                ProfilePictureBrush.ImageSource = bitmapImage;
            }
            catch
            {
            }

            AccountUsername.Text = loggedUsername;
            AccountAdditionalInfo.Text = user.AdditionalInfo;

            VerificationTab.Visibility = Visibility.Collapsed;
            AccountTab.Visibility = Visibility.Visible;
            AccountSettingsTab.Visibility = Visibility.Visible;
            tabControl.SelectedItem = AccountTab;
            MessageBox.Show("Email verified successfully! Registration complete.");
        }
        private void ResendCode_Click(object sender, RoutedEventArgs e)
        {
            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u =>
                u.Email == _pendingVerificationEmail &&
                u.Username == _pendingVerificationUsername);

            if (user == null) return;

            var random = new Random();
            string newCode = random.Next(100000, 999999).ToString();
            DateTime expiryTime = DateTime.Now.AddMinutes(5);

            user.VerificationCode = newCode;
            user.VerificationCodeExpiry = expiryTime;
            db.SaveChanges();

            try
            {
                new EmailService().SendVerificationEmail(_pendingVerificationEmail, newCode);
                VerificationStatus.Text = "New code sent! Check your email.";
            }
            catch (Exception ex)
            {
                VerificationStatus.Text = $"Failed to resend code: {ex.Message}";
            }
        }
        private void UploadImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                string resourcesFolder = System.IO.Path.Combine(Environment.CurrentDirectory, "resources");
                string tempImagePath = System.IO.Path.Combine(resourcesFolder, "temp.jpg");

                if (!Directory.Exists(resourcesFolder))
                {
                    Directory.CreateDirectory(resourcesFolder);
                }

                try
                {
                    if (File.Exists(tempImagePath))
                    {
                        File.Delete(tempImagePath);
                    }

                    File.Copy(filePath, tempImagePath);

                    BitmapImage bitmapImage = new BitmapImage();
                    using (var stream = new FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = stream;
                        bitmapImage.EndInit();
                    }

                    ImagePreviewBrush.ImageSource = bitmapImage;
                    ImagePreview.Visibility = Visibility.Visible;
                    SwitchPanel.Visibility = Visibility.Visible;
                    RegisterImageMessage.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while uploading the image: {ex.Message}");
                }
            }
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            ImagePreviewBrush.Stretch = Stretch.UniformToFill;
        }

        private void ToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            ImagePreviewBrush.Stretch = Stretch.Fill;
        }

        private void SaveTempImage(bool crop, string name)
        {
            try
            {
                string resourcesFolder = System.IO.Path.Combine(Environment.CurrentDirectory, "resources");
                string profilePicsFolder = System.IO.Path.Combine(resourcesFolder, "profile_pics");
                string tempImagePath = System.IO.Path.Combine(resourcesFolder, "temp.jpg");
                string targetImagePath = System.IO.Path.Combine(profilePicsFolder, $"{name}.jpg");

                if (!Directory.Exists(profilePicsFolder))
                {
                    Directory.CreateDirectory(profilePicsFolder);
                }

                if (!File.Exists(tempImagePath))
                {
                    MessageBox.Show("No image found. Please upload an image first.");
                    return;
                }

                BitmapImage bitmapImage = new BitmapImage();
                using (var stream = new FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }

                int size = Math.Min(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                int x = (bitmapImage.PixelWidth - size) / 2;
                int y = (bitmapImage.PixelHeight - size) / 2;

                RenderTargetBitmap renderTarget;
                if (crop)
                {
                    CroppedBitmap croppedBitmap = new CroppedBitmap(bitmapImage, new Int32Rect(x, y, size, size));
                    renderTarget = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(croppedBitmap, new Rect(0, 0, size, size));
                    }
                    renderTarget.Render(drawingVisual);
                }
                else
                {
                    int squareSize = size;
                    renderTarget = new RenderTargetBitmap(squareSize, squareSize, 96, 96, PixelFormats.Pbgra32);
                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(bitmapImage, new Rect(0, 0, squareSize, squareSize));
                    }
                    renderTarget.Render(drawingVisual);
                }

                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                using (FileStream fileStream = new FileStream(targetImagePath, FileMode.Create, FileAccess.Write))
                {
                    encoder.Save(fileStream);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving the image: {ex.Message}");
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double x = e.NewSize.Width;
            double y = e.NewSize.Height;

            double h = Math.Min((y - 400) / 2, (x - 200) / 2);
            double j = Math.Min(Math.Max(100, h), 200);

            ImagePreview.Width = j;
            ImagePreview.Height = j;
        }

        private void LoginUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoginUsernameMessage.Visibility = Visibility.Collapsed;
        }

        private void LoginPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LoginPasswordMessage.Visibility = Visibility.Collapsed;
        }

        private void RegisterUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterUsernameMessage.Visibility = Visibility.Collapsed;
        }

        private void RegisterEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterEmailMessage.Visibility = Visibility.Collapsed;
        }

        private void RegisterPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            RegisterPasswordMessage.Visibility = Visibility.Collapsed;
        }

        private void RegisterRepeatPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            RegisterRepeatMessage.Visibility = Visibility.Collapsed;
        }

        private void RegisterAdditionalInfo_TextChanged(object sender, TextChangedEventArgs e)
        {
            RegisterAdditionalMessage.Visibility = Visibility.Collapsed;
        }

        private void ChangePasswordOld_PasswordChanged(object sender, RoutedEventArgs e)
        {
            AccountSettingsOldMessage.Visibility = Visibility.Collapsed;
        }

        private void ChangePasswordNew_PasswordChanged(object sender, RoutedEventArgs e)
        {
            AccountSettingsNewMessage.Visibility = Visibility.Collapsed;
        }

        private void ChangePasswordRepeat_PasswordChanged(object sender, RoutedEventArgs e)
        {
            AccountSettingsRepeatMessage.Visibility = Visibility.Collapsed;
        }


        private void ResetLoginTab()
        {
            LoginUsername.Text = "";
            LoginPassword.Password = "";
            LoginUsernameMessage.Visibility = Visibility.Collapsed;
            LoginPasswordMessage.Visibility = Visibility.Collapsed;
        }

        private void ResetRegisterTab()
        {
            RegisterUsername.Text = "";
            RegisterPassword.Password = "";
            RegisterRepeatPassword.Password = "";
            RegisterAdditionalInfo.Text = "";
            RegisterEmail.Text = "";
            RegisterEmailMessage.Visibility = Visibility.Collapsed;
            RegisterUsernameMessage.Visibility = Visibility.Collapsed;
            RegisterPasswordMessage.Visibility = Visibility.Collapsed;
            RegisterRepeatMessage.Visibility = Visibility.Collapsed;
            RegisterAdditionalMessage.Visibility = Visibility.Collapsed;
            RegisterImageMessage.Visibility = Visibility.Collapsed;
            ImagePreview.Visibility = Visibility.Collapsed;
            SwitchPanel.Visibility = Visibility.Collapsed;
        }

        private void ResetAccountTabs()
        {
            AccountUsername.Text = "";
            AccountAdditionalInfo.Text = "";
            AccountSettingsOldMessage.Visibility = Visibility.Collapsed;
            AccountSettingsNewMessage.Visibility = Visibility.Collapsed;
            AccountSettingsRepeatMessage.Visibility = Visibility.Collapsed;
            AccountSettingsOldPassword.Password = "";
            AccountSettingsNewPassword.Password = "";
            AccountSettingsRepeatPassword.Password = "";
        }


        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string oldPassword = AccountSettingsOldPassword.Password;
            string newPassword = AccountSettingsNewPassword.Password;
            string repeatPassword = AccountSettingsRepeatPassword.Password;

            bool failed = false;

            if (oldPassword == "")
            {
                failed = true;
                AccountSettingsOldMessage.Text = "Old password cannot be empty!";
                AccountSettingsOldMessage.Visibility = Visibility.Visible;
            }
            if (newPassword == "")
            {
                failed = true;
                AccountSettingsNewMessage.Text = "New password cannot be empty!";
                AccountSettingsNewMessage.Visibility = Visibility.Visible;
            }
            if (repeatPassword == "")
            {
                failed = true;
                AccountSettingsRepeatMessage.Text = "Repeat password cannot be empty!";
                AccountSettingsRepeatMessage.Visibility = Visibility.Visible;
            }
            else if (newPassword != repeatPassword)
            {
                failed = true;
                AccountSettingsRepeatMessage.Text = "Passwords do not match!";
                AccountSettingsRepeatMessage.Visibility = Visibility.Visible;
            }

            if (failed) return;

            using var db = new AppDbContext();
            var user = db.Users.FirstOrDefault(u => u.Username == loggedUsername);

            if (user == null)
            {
                MessageBox.Show("WTF");
                return;
            }

            string pepperedOldPassword = oldPassword + SecurityConfig.Pepper;
            if (!BCrypt.Net.BCrypt.Verify(pepperedOldPassword, user.PasswordHash))
            {
                AccountSettingsOldMessage.Text = "Incorrect old password!";
                AccountSettingsOldMessage.Visibility = Visibility.Visible;
                return;
            }

            string pepperedNewPassword = newPassword + SecurityConfig.Pepper;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(pepperedNewPassword, workFactor: 12);
            db.SaveChanges();

            AccountSettingsOldPassword.Password = "";
            AccountSettingsNewPassword.Password = "";
            AccountSettingsRepeatPassword.Password = "";

            MessageBox.Show("Password changed successfully!");
            ResetAccountTabs();
        }


        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            loggedIn = false;
            loggedUsername = "";
            loggedPassword = "";

            LoginTab.Visibility = Visibility.Visible;
            RegisterTab.Visibility = Visibility.Visible;

            AccountTab.Visibility = Visibility.Collapsed;
            AccountSettingsTab.Visibility = Visibility.Collapsed;

            tabControl.SelectedItem = LoginTab;

            ResetAccountTabs();


        }



        private string GetVerificationCode()
        {
            return CodeBox1.Text +
                   CodeBox2.Text +
                   CodeBox3.Text +
                   CodeBox4.Text +
                   CodeBox5.Text +
                   CodeBox6.Text;
        }
        private void VerificationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var currentBox = sender as TextBox;
            if (currentBox == null) return;

            if (currentBox.Text.Length == 1)
            {
                MoveToNextBox(currentBox);
            }
        }

        private void MoveToNextBox(TextBox currentBox)
        {
            var index = int.Parse(currentBox.Name.Substring(7)) - 1;
            if (index < 5)
            {
                var nextBox = FindName($"CodeBox{index + 2}") as TextBox;
                nextBox?.Focus();
            }
        }

        private void MoveToPreviousBox(TextBox currentBox)
        {
            var index = GetBoxIndex(currentBox);
            if (index > 0)
            {
                var prevBox = FindName($"CodeBox{index}") as TextBox;
                if (prevBox != null)
                {
                    prevBox.Text = "";
                    prevBox.Focus();
                    prevBox.CaretIndex = 0;
                }
            }
        }
        private int GetBoxIndex(TextBox box)
        {
            return int.Parse(box.Name.Substring(7)) - 1;
        }
        private void VerificationBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox box)
            {
                box.BorderBrush = new SolidColorBrush(Colors.Blue);
                box.Background = new SolidColorBrush(Color.FromArgb(15, 33, 150, 243));
                box.CaretIndex = box.Text.Length;
                box.Select(0, 0);
                if (string.IsNullOrEmpty(box.Text))
                {
                    box.SelectAll();
                }
                else
                {
                    box.CaretIndex = box.Text.Length; 
                }
            }
        
        }

        private void VerificationBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox box)
            {
                box.BorderBrush = new SolidColorBrush(Color.FromRgb(204, 204, 204));
                box.Background = new SolidColorBrush(Colors.White);
            }
        }
        private void VerificationBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
        private void VerificationBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var currentBox = sender as TextBox;
            if (currentBox == null) return;

            if (e.Key == Key.Back)
            {
                if (string.IsNullOrEmpty(currentBox.Text))
                {
                    MoveToPreviousBox(currentBox);
                    e.Handled = true; 
                }
            }
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                VerifyCode_Click(sender, e);
            }
        }

    }
}