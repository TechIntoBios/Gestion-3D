namespace DTOs;

public class LoginResponse
    {
        public String? Token { get; set; } 
        public String? Username { get; set; }
        public int ExpiraEnHoras { get; set; }
    }