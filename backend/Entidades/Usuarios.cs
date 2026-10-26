namespace Entidades;

public class Usuarios
{
	private Guid idUsuario;
	private string usuario;
	private string password;
	private DateTimeOffset fechaCreacion;
	private bool activo;

	public Usuarios()
	{
		idUsuario = Guid.Empty;
		usuario = "Sin definir";
		password = "Sin definir";
		fechaCreacion = DateTimeOffset.UtcNow;
		activo = true;
	}

	public Usuarios(Guid idUsuario, string usuario, string password, DateTimeOffset fechaCreacion, bool activo)
	{
		this.idUsuario = idUsuario;
		this.usuario = usuario;
		this.password = password;
		this.fechaCreacion = fechaCreacion;
		this.activo = activo;
	}

	public Guid IdUsuario
	{
		get { return idUsuario; }
		set { idUsuario = value; }
	}

	public string Usuario
	{
		get { return usuario; }
		set { usuario = value; }
	}

	public string Password
	{
		get { return password; }
		set { password = value; }
	}

	public DateTimeOffset FechaCreacion
	{
		get { return fechaCreacion; }
		set { fechaCreacion = value; }
	}

	public bool Activo
	{
		get { return activo; }
		set { activo = value; }
	}
}
