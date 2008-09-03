<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="CharacterConverter.aspx.cs" Inherits="CharacterConverterView" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Character converter</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table >
    <tr>
    <td>Db host:</td>
    <td><asp:TextBox ID="TextBox1" runat="server">localhost</asp:TextBox></td>
    </tr>
    <tr>
    <td>Db port:</td>
    <td><asp:TextBox ID="TextBox2" runat="server">3306</asp:TextBox></td>
    </tr>
    <tr>
    <td>Db base:</td>
    <td><asp:TextBox ID="TextBox3" runat="server"></asp:TextBox></td>
    </tr>
    <tr>
    <td>Db user:</td>
    <td><asp:TextBox ID="TextBox4" runat="server"></asp:TextBox></td>
    </tr>
    <tr>
    <td>Db pass:</td>
    <td><asp:TextBox ID="TextBox5" runat="server"></asp:TextBox></td>
    </tr>
    <td colspan = "2" >
		<asp:Button ID="Button1" runat="server" onclick="Button1_Click" Text="Button" />
    </td>
    <tr>
    </tr>
    </table>
    </div>
    </form>
</body>
</html>
