<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebParts._Default" %>

<%@ Register Src="WebControl.ascx" TagName="WebControl" TagPrefix="uc1" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>ASP.NET</h1>
        <p class="lead">ASP.NET is a free web framework for building great Web sites and Web applications using HTML, CSS, and JavaScript.</p>
        <p><a href="http://www.asp.net" class="btn btn-primary btn-large">Learn more &raquo;</a></p>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Getting started</h2>
            <p>
                ASP.NET Web Forms lets you build dynamic websites using a familiar drag-and-drop, event-driven model.
            A design surface and hundreds of controls and components let you rapidly build sophisticated, powerful UI-driven sites with data access.
            </p>
            <p>
                <a class="btn btn-default" href="http://go.microsoft.com/fwlink/?LinkId=301948">Learn more &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Get more libraries</h2>
            <p>
                NuGet is a free Visual Studio extension that makes it easy to add, remove, and update libraries and tools in Visual Studio projects.
            </p>
            <p>
                <a class="btn btn-default" href="http://go.microsoft.com/fwlink/?LinkId=301949">Learn more &raquo;</a>
            </p>
        </div>
        <div class="col-md-4">
            <h2>Web Hosting</h2>
            <p>
                You can easily find a web hosting company that offers the right mix of features and price for your applications.
            </p>
            <p>
                <a class="btn btn-default" href="http://go.microsoft.com/fwlink/?LinkId=301950">Learn more &raquo;</a>
            </p>
        </div>
    </div>
    <div>
        <asp:WebPartManager ID="WebPartManager1" runat="server"></asp:WebPartManager>

        <asp:Table ID="Table1" runat="server" Width="1264px">
            <asp:TableRow runat="server">
                <asp:TableCell runat="server">
                    <asp:WebPartZone ID="WebPartZone1" runat="server" HeaderText="Side">
                        <ZoneTemplate>
                            <asp:Label runat="server" ID="linksPart" title="My Links">
      <a href="http://www.asp.net">ASP.NET site</a> 
      <br />
      <a href="http://www.gotdotnet.com">GotDotNet</a> 
      <br />
      <a href="http://www.contoso.com">Contoso.com</a> 
      <br />
                            </asp:Label><uc1:WebControl ID="WebControl1" runat="server" />
                        </ZoneTemplate>
                    </asp:WebPartZone>
                </asp:TableCell>
                <asp:TableCell runat="server">
                    <asp:WebPartZone ID="WebPartZone2" runat="server"></asp:WebPartZone>
                </asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    </div>
</asp:Content>
