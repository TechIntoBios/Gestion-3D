namespace DTOs;

// El token nunca viaja en el cuerpo: va solo en la cookie HttpOnly.
public class LoginResponse
{
    public string? Usuario { get; set; }
    public int ExpiraEnHoras { get; set; }
}
