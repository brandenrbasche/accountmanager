//and we need this to manipulate data from a db
using System.Data;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
//we need these to talk to mysql
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace accountmanager
{
	/// <summary>
	/// Summary description for AccountServices
	/// </summary>
	[WebService(Namespace = "http://tempuri.org/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	[System.ComponentModel.ToolboxItem(false)]
	// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
	[System.Web.Script.Services.ScriptService]
	public class AccountServices : System.Web.Services.WebService
	{

		//EXAMPLE OF A SIMPLE SELECT QUERY (PARAMETERS PASSED IN FROM CLIENT)
		[WebMethod(EnableSession = true)] //NOTICE: gotta enable session on each individual method
		public bool LogOn(string uid, string pass)
		{
			//we return this flag to tell them if they logged in or not
			bool success = false;

			//our connection string comes from our web.config file like we talked about earlier
			string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["pentest"].ConnectionString;
			//here's our query.  A basic select with nothing fancy.  Note the parameters that begin with @
			//NOTICE: we added admin to what we pull, so that we can store it along with the id in the session
			string sqlSelect = "SELECT user_ID, Password FROM User_Account WHERE user_ID=@user_IDValue and Password=@PasswordValue";

			//set up our connection object to be ready to use our connection string
			MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
			//set up our command object to use our connection, and our query
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			//tell our command to replace the @parameters with real values
			//we decode them because they came to us via the web so they were encoded
			//for transmission (funky characters escaped, mostly)
			sqlCommand.Parameters.AddWithValue("@user_IDValue", HttpUtility.UrlDecode(uid));
			sqlCommand.Parameters.AddWithValue("@PasswordValue", HttpUtility.UrlDecode(pass));

			//a data adapter acts like a bridge between our command object and 
			//the data we are trying to get back and put in a table object
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//here's the table we want to fill with the results from our query
			DataTable sqlDt = new DataTable();
			//here we go filling it!
			sqlDa.Fill(sqlDt);
			//check to see if any rows were returned.  If they were, it means it's 
			//a legit account
			if (sqlDt.Rows.Count > 0)
			{
				//if we found an account, store the id and admin status in the session
				//so we can check those values later on other method calls to see if they 
				//are 1) logged in at all, and 2) and admin or not
				Session["user_ID"] = sqlDt.Rows[0]["user_ID"];
				//Session["admin"] = sqlDt.Rows[0]["admin"];
				success = true;
			}
			//return the result!
			return success;
		}

		[WebMethod(EnableSession = true)]
		public bool LogOff()
		{
			//if they log off, then we remove the session.  That way, if they access
			//again later they have to log back on in order for their ID to be back
			//in the session!
			Session.Abandon();
			return true;
		}

		//EXAMPLE OF AN INSERT QUERY WITH PARAMS PASSED IN.  BONUS GETTING THE INSERTED ID FROM THE DB!
		[WebMethod(EnableSession = true)]
		public void RequestAccount(string user_ID, string University_name, string Fname, string Lname, string Password, string Major)//, int AdminStatus)
		{
			string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["pentest"].ConnectionString;
			//the only thing fancy about this query is SELECT LAST_INSERT_ID() at the end.  All that
			//does is tell mySql server to return the primary key of the last inserted row.
			string sqlSelect = "INSERT INTO User_Accounts (user_ID, University_name, Fname, Lname, Password, Major) " +
				"VALUES (@user_IDValue, @University_nameValue, @FnameValue, @LnameValue, @PasswordValue, @MajorValue); ";

			//"SELECT LAST_INSERT_ID();";
			MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			sqlCommand.Parameters.AddWithValue("@user_IDValue", user_ID);
			sqlCommand.Parameters.AddWithValue("@University_nameValue", University_name);
			sqlCommand.Parameters.AddWithValue("@FnameValue", Fname);
			sqlCommand.Parameters.AddWithValue("@LnameValue", Lname);
			sqlCommand.Parameters.AddWithValue("@PasswordValue", Password);
			sqlCommand.Parameters.AddWithValue("@MajorValue", Major);

			//this time, we're not using a data adapter to fill a data table.  We're just
			//opening the connection, telling our command to "executescalar" which says basically
			//execute the query and just hand me back the number the query returns (the ID, remember?).
			//don't forget to close the connection!
			sqlConnection.Open();
			//we're using a try/catch so that if the query errors out we can handle it gracefully
			//by closing the connection and moving on
			try
			{
				sqlCommand.ExecuteNonQuery();
				//int account_ID = Convert.ToInt32(sqlCommand.ExecuteScalar());
				//sqlCommand.ExecuteScalar();
				//here, you could use this accountID for additional queries regarding
				//the requested account.  Really this is just an example to show you
				//a query where you get the primary key of the inserted row back from
				//the database!
			}
			catch (Exception e)
			{
			}
			sqlConnection.Close();
		}

		// METHOD TO HANDLE GRADE ENTRY DATA
		[WebMethod(EnableSession = true)]
		public void HandleGradeEntryData(int Year, string Term, int Total_Credits, double CurrentGPA)
		{
			string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["pentest"].ConnectionString;

			string sqlSelect = "INSERT INTO Grades (Year, Term, Total_Credits, CurrentGPA) " +
				"VALUES (@yearValue, @termValue, @totalCreditsValue, @currentGpaValue);";

			//"SELECT LAST_INSERT_ID();";
			MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

			sqlCommand.Parameters.AddWithValue("@yearValue", Year);
			sqlCommand.Parameters.AddWithValue("@termValue", Term);
			sqlCommand.Parameters.AddWithValue("@totalCreditsValue", Total_Credits);
			sqlCommand.Parameters.AddWithValue("@currentGpaValue", CurrentGPA);

			sqlConnection.Open();

			try
			{
				sqlCommand.ExecuteNonQuery();
			}
			catch (Exception e)
			{
			}
			sqlConnection.Close();
		}

		//EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
		[WebMethod(EnableSession = true)]
		//[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
		public void GetAccounts()
		{
			//check out the return type. It's an array of Account objects. You can look at our custom Account class in this solution to see that it's 
			//just a container for public class-level variables. It's a simple container that asp.net will have no trouble converting into json. When we return
			//sets of information, it's a good idea to create a custom container class to represent instances (or rows) of that information, and then return an array of those objects.  
			//Keeps everything simple.
			//LOGIC: get all the active accounts and return them!
			DataTable sqlDt = new DataTable("grades");
			string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["pentest"].ConnectionString;
			string sqlSelect = "select Year, Term, Total_Credits, user_ID, CurrentGPA from Grades order by user_ID";
			MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
			MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);
			//gonna use this to fill a data table
			MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
			//filling the data table
			sqlDa.Fill(sqlDt);
			//loop through each row in the dataset, creating instances
			//of our container class Account. Fill each acciount with
			//data from the rows, then dump them in a list.
			List<Grade> grades = new List<Grade>();
			for (int i = 0; i < sqlDt.Rows.Count; i++)
			{
				grades.Add(new Grade
				{
					Year = Convert.ToInt32(sqlDt.Rows[i]["Year"]),
					user_ID = Convert.ToInt32(sqlDt.Rows[i]["user_ID"]),
					Term = sqlDt.Rows[i]["Term"].ToString(),
					Total_Credits = Convert.ToDouble(sqlDt.Rows[i]["Total_Credits"]),
					CurrentGPA = Convert.ToDouble(sqlDt.Rows[i]["CurrentGPA"]),
				});
			}
			//convert the list of accounts to an array and return!
			System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
			this.Context.Response.ContentType = "application/json; charset=utf-8";
			this.Context.Response.Write(jss.Serialize(grades.ToArray()));
		}

		//EXAMPLE OF AN UPDATE QUERY WITH PARAMS PASSED IN
		[WebMethod(EnableSession = true)]
		public void UpdateAccount(string id, string uid, string pass, string firstName, string lastName, string email)
		{
			//WRAPPING THE WHOLE THING IN AN IF STATEMENT TO CHECK IF THEY ARE AN ADMIN!
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				//this is a simple update, with parameters to pass in values
				string sqlSelect = "update accounts set userid=@uidValue, pass=@passValue, firstname=@fnameValue, lastname=@lnameValue, " +
					"email=@emailValue where id=@idValue";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@uidValue", HttpUtility.UrlDecode(uid));
				sqlCommand.Parameters.AddWithValue("@passValue", HttpUtility.UrlDecode(pass));
				sqlCommand.Parameters.AddWithValue("@fnameValue", HttpUtility.UrlDecode(firstName));
				sqlCommand.Parameters.AddWithValue("@lnameValue", HttpUtility.UrlDecode(lastName));
				sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
				sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

				sqlConnection.Open();
				//we're using a try/catch so that if the query errors out we can handle it gracefully
				//by closing the connection and moving on
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}









		//EXAMPLE OF A SELECT, AND RETURNING "COMPLEX" DATA TYPES
		[WebMethod(EnableSession = true)]
		public Account[] GetAccountRequests()
		{//LOGIC: get all account requests and return them!
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				DataTable sqlDt = new DataTable("accountrequests");

				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				//requests just have active set to 0
				string sqlSelect = "select id, userid, pass, firstname, lastname from accounts where active=0 order by lastname";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
				sqlDa.Fill(sqlDt);

				List<Account> accountRequests = new List<Account>();
				for (int i = 0; i < sqlDt.Rows.Count; i++)
				{
					accountRequests.Add(new Account
					{
						//uid = Convert.ToInt32(sqlDt.Rows[i]["user_ID"]),
						uid = sqlDt.Rows[i]["uid"].ToString(),
						firstName = sqlDt.Rows[i]["firstname"].ToString(),
						lastName = sqlDt.Rows[i]["lastname"].ToString()
					}); ;
				}
				//convert the list of accounts to an array and return!
				return accountRequests.ToArray();
			}
			else
			{
				return new Account[0];
			}
		}

		//EXAMPLE OF A DELETE QUERY
		[WebMethod(EnableSession = true)]
		public void DeleteAccount(string id)
		{
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				//this is a simple update, with parameters to pass in values
				string sqlSelect = "delete from accounts where id=@idValue";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

				sqlConnection.Open();
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}

		//EXAMPLE OF AN UPDATE QUERY
		[WebMethod(EnableSession = true)]
		public void ActivateAccount(string id)
		{
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				//this is a simple update, with parameters to pass in values
				string sqlSelect = "update accounts set active=1 where id=@idValue";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

				sqlConnection.Open();
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}

		//EXAMPLE OF A DELETE QUERY
		[WebMethod(EnableSession = true)]
		public void RejectAccount(string id)
		{
			if (Convert.ToInt32(Session["admin"]) == 1)
			{
				string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
				string sqlSelect = "delete from accounts where id=@idValue";

				MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
				MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

				sqlCommand.Parameters.AddWithValue("@idValue", HttpUtility.UrlDecode(id));

				sqlConnection.Open();
				try
				{
					sqlCommand.ExecuteNonQuery();
				}
				catch (Exception e)
				{
				}
				sqlConnection.Close();
			}
		}


	}
}
