
Imports MySql.Data.MySqlClient
Imports System.Collections.Specialized


Public Class Mys

#Region "Properties"

    Protected disposed As Boolean = False

    Private _ConnectionString As String

    Sub New()
        ' TODO: Complete member initialization 
    End Sub

    Public Property ConnectionString() As String
        Get
            Return _ConnectionString
        End Get
        Set(value As String)
            _ConnectionString = value
        End Set
    End Property

    Private _Connection As MySqlConnection
    Public Property Connection() As MySqlConnection
        Get
            Return _Connection
        End Get
        Set(value As MySqlConnection)
            _Connection = value
        End Set
    End Property

    Private _Cmd As String
    Public Property Cmd() As String
        Get
            Return _Cmd
        End Get
        Set(value As String)
            _Cmd = value
        End Set
    End Property

#End Region


    Public Sub Mys()

    End Sub

    Public Sub New(ByVal ConStr As String)
        Try
            ConnectionString = ConStr
            Connection = New MySqlConnection(ConnectionString)
            OpenConnection()
        Catch ex As Exception
            'Console.WriteLine(ex.Message)
        End Try
    End Sub

    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposed Then
            CloseConnection()
        End If
        'Me.disposed = True
    End Sub

    Public Sub Dispose() ' Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub


    Public Sub OpenConnection()
        Try
            Connection.Open()
            'Console.WriteLine("Connection was Opened")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Public Sub CloseConnection()
        Try
            Connection.Close()
            'Console.WriteLine("Connection was Closed")
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try
    End Sub

    Private Sub DoForAllQuery()
        If Not (Connection.State = System.Data.ConnectionState.Open) Then OpenConnection()
    End Sub

    Public Function Sql_Query_DataReader(ByVal StrSql As String) As MySqlDataReader
        Dim DR As MySqlDataReader
        Try
            Dim Command As MySqlCommand = New MySqlCommand(StrSql, Connection)
            DR = Command.ExecuteReader
            Return DR
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
        End Try
        Return Nothing
    End Function

    Public Function Sql_Query_DataAdapter(ByVal StrSql As String) As MySqlDataAdapter
        Dim DA As MySqlDataAdapter
        Try
            Dim Command As MySqlCommand = New MySqlCommand(StrSql, Connection)
            DA = New MySqlDataAdapter(Command)
            Return DA
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
        Finally

        End Try
        Return Nothing
    End Function

    Public Function Sql_Execute_Non_Query(ByVal StrSql As String) As MySqlCommand
        Try
            Dim Command As MySqlCommand = New MySqlCommand
            Command = New MySqlCommand(StrSql, Connection)
            Command.ExecuteNonQuery()
            Return Command
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
        End Try
        Return Nothing
    End Function
    Public Function Sql_Execute_Non_Query_withTrueFalseReturn(ByVal StrSql As String) As Boolean
        Try
            Dim Command As MySqlCommand
            Command = New MySqlCommand(StrSql, Connection)
            Command.ExecuteNonQuery()
            Return True
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
            Return False
        End Try
        Return "True"
    End Function

    Public Function Sql_Execute_Non_Query_Result(ByVal StrSql As String) As Boolean
        Dim ReturnVal As Boolean = False
        Try
            Dim Command As MySqlCommand = New MySqlCommand(StrSql, Connection)
            If Not Command.ExecuteNonQuery = 0 Then
                ReturnVal = True
            Else
                ReturnVal = False
            End If
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
        End Try
        Return ReturnVal
    End Function

    Public Sub Sql_Execute_Non_Query_NoResult(ByVal StrSql As String)
        Try
            Dim Command As MySqlCommand = New MySqlCommand(StrSql, Connection)
            Command.ExecuteNonQuery()
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
        End Try
    End Sub

    Public Function Sql_Execute_Non_Query_RowAffected(ByVal StrSql As String) As Integer
        Dim NumberOfRowAffected As Integer = 0
        Try
            Dim Command As MySqlCommand = New MySqlCommand(StrSql, Connection)
            NumberOfRowAffected = Command.ExecuteNonQuery()
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
        End Try
        Return NumberOfRowAffected
    End Function

    Public Function Sql_Execute_Non_Query_ErrorResult(ByVal StrSql As String) As String
        Try
            Dim Command As MySqlCommand = New MySqlCommand(StrSql, Connection)
            Command.ExecuteNonQuery()
            Return "ok"
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Private Sub ErrLoger(ByVal ConnectionSettings As String, ByVal ErrX As String, Optional ByVal UserName As String = Nothing)
        'Dim HLP As Pg = New Pg
        'Dim StrSql As String = Nothing
        'StrSql = "INSERT INTO errors (error ,username ,datetime) VALUES('" + ErrX + "' ,'" + UserName + "' ," + System.DateTime.Now.Date + ");"
        ''HLP.Sql_Execute_Non_Query_NoResult(StrSql, ConnectionSettings, "SharpMap")
    End Sub

    Private Function ConnectionSettings_Spliter(ByVal ConnectionSettings As String, ByVal DataBaseName As String) As String
        Dim CS_Splited(ConnectionSettings.Split(";").Length - 2) As String
        CS_Splited = ConnectionSettings.Split(";")
        CS_Splited(4) = "Database=" + DataBaseName
        Dim B_For_Ring As Byte
        Dim CS_Completed As String = Nothing
        For B_For_Ring = 0 To CS_Splited.Length - 2
            CS_Completed = String.Format("{0}{1};", CS_Completed, CS_Splited(B_For_Ring))
        Next
        Return CS_Completed
    End Function

    'Must edit for MySql
    Public Function GetColumnsName(ByVal TableName As String) As MySqlDataReader
        Dim StrSql As String = Nothing
        StrSql = String.Format("select column_name from information_schema.columns where table_name = '{0}';", TableName)
        Return Sql_Query_DataReader(StrSql)
    End Function

    Public Sub RemoveColumn(ByVal TableName As String, ByVal FieldName As String)
        Dim StrSql As String = Nothing
        StrSql = String.Format("ALTER TABLE {0} DROP COLUMN {1};",
                     TableName,
                     FieldName)
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Sub AddColumn(ByVal TableName As String, ByVal FieldName As String, ByVal FieldType As String)
        Dim StrSql As String = Nothing
        StrSql = String.Format("ALTER TABLE {0} ADD {1} {2};", TableName, FieldName, FieldType)
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Sub AddRecord(ByVal TableName As String, ByVal FieldsName As String, ByVal Values As String)
        Dim StrSql As String = Nothing
        StrSql = String.Format("INSERT INTO {0}({1}) VALUES({2});", TableName, FieldsName, Values)
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Sub DeleteRecord(ByVal TableName As String, ByVal Where As String)
        Dim StrSql As String = Nothing
        StrSql = String.Format("DELETE FROM {0} where {1};", TableName, Where)
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Function DeleteRecord_WithReturnStatuse(ByVal TableName As String, ByVal Where As String) As Boolean
        Dim StrSql As String = Nothing
        StrSql = String.Format("DELETE FROM {0} where {1};", TableName, Where)
        Return Sql_Execute_Non_Query_Result(StrSql)
    End Function

    Public Sub DeleteAllRecords(ByVal TableName As String)
        Dim StrSql As String = Nothing
        StrSql = String.Format("TRUNCATE {0};", TableName)
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Sub UpdateRecord(ByVal TableName As String, ByVal Update As String, ByVal Where As String)
        Dim StrSql As String = Nothing
        Try
            If Where = Nothing Then
                StrSql = String.Format("UPDATE {0} SET {1};", TableName, Update)
            Else
                StrSql = String.Format("UPDATE {0} SET {1} Where {2};", TableName, Update, Where)
            End If
            Sql_Execute_Non_Query_NoResult(StrSql)
        Catch ex As Exception
            Console.WriteLine("strSQL = " + StrSql + " ||| \n Message = " + ex.Message)
        End Try
    End Sub

    Public Sub SearchBySqlCommand(ByVal ConnectionSettings As String, ByVal SqlCommand As String)
        Dim StrSql As String = Nothing
        StrSql = SqlCommand
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Function CountOfRecords(ByVal TableName As String) As Integer
        Dim StrSql As String = Nothing
        StrSql = "select count(*) from " + TableName
        Dim DR As MySqlDataReader = Sql_Query_DataReader(StrSql)
        DR.Read()
        Dim retval As Integer = Val(DR(0).ToString())
        DR.Close()
        Return retval
    End Function
    Public Function CountOfRecordsWhere(ByVal TableName As String, ByVal Where As String) As Integer
        Dim StrSql As String = Nothing
        If Where <> Nothing Then
            StrSql = String.Format("select count(*) from {0} where {1}", TableName, Where)
        Else
            StrSql = String.Format("select count(*) from {0}", TableName)
        End If
        Dim DR As MySqlDataReader = Sql_Query_DataReader(StrSql)
        DR.Read()
        Dim retval As Integer = Val(DR(0).ToString())
        DR.Close()
        Return retval
    End Function

    ''' <summary>
    ''' If a value exist in a table then return True
    ''' </summary>
    ''' <param name="TabelName"></param>
    ''' <param name="FieldName"></param>
    ''' <param name="FieldValue"></param>
    ''' <returns>If a value exist in a table then return True</returns>
    ''' <remarks></remarks>
    Public Function IsThisValueExist(ByVal TabelName As String, ByVal FieldName As String, ByVal FieldValue As String) As Boolean
        Dim count As Integer = CountOfRecordsWhere(TabelName, String.Format("{0} = '{1}'", FieldName, FieldValue))
        If count <= 0 Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Sub CreateTable(ByVal TableName As String, Optional ByVal Fields As String = Nothing)
        Dim StrSql As String = Nothing
        If Fields <> Nothing Then
            StrSql = String.Format("CREATE TABLE {0} ({1});", TableName, Fields)
        Else
            StrSql = String.Format("CREATE TABLE {0};", TableName)
        End If
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Sub DropTable_S(ByVal Table_S_Name As String)
        Dim StrSql As String = Nothing
        StrSql = String.Format("DROP TABLE {0};", Table_S_Name)
        Sql_Execute_Non_Query_NoResult(StrSql)
    End Sub

    Public Function GetTable(ByVal SqlStr As String) As DataTable
        Dim DT As DataTable = New DataTable
        Try
            Dim DA As MySqlDataAdapter = Sql_Query_DataAdapter(SqlStr)
            DA.Fill(DT)
        Catch ex As Exception
        End Try
        Return DT
    End Function

    Public Function GetDataSet(ByVal SqlStr As String, ByVal TableName As String) As DataSet
        Dim DS As DataSet = New DataSet
        Try
            Dim DA As MySqlDataAdapter = Sql_Query_DataAdapter(SqlStr)
            DA.Fill(DS, TableName)
        Catch ex As Exception
        End Try
        Return DS
    End Function

    Public Function GetDataView(ByVal SqlStr As String, ByVal TableName As String) As DataView
        Dim DV As DataView = New DataView
        Try
            DV = New DataView(GetDataSet(SqlStr, TableName).Tables(TableName))
        Catch ex As Exception
        End Try
        Return DV
    End Function

    Public Function GetLastRecords(ByVal TableName As String, ByVal NumberOfLastRecords As String, Optional ByVal FieldToReturn As String = "*") As MySqlDataReader
        Dim DR As MySqlDataReader
        Try
            DR = Sql_Query_DataReader(String.Format("select {0} from {1} order by id desc limit {2}", FieldToReturn, TableName, NumberOfLastRecords))
            Return DR
        Catch ex As Exception
            DR.Close()
        Finally
            DR.Close()
        End Try
        Return DR
    End Function

    Public Function GetLastRecord(ByVal TableName As String, ByVal id As String, Optional ByVal FieldToReturn As String = "*") As String
        Dim DR As MySqlDataReader = Sql_Query_DataReader(String.Format("select {0} from {1} order by {2} desc limit 1", FieldToReturn, TableName, id))
        Dim RetVal As String = ""
        Try
            DR.Read()
            RetVal = DR(0).ToString()
            DR.Close()
        Catch ex As Exception
            DR.Close()
        End Try
        Return RetVal
    End Function

    Public Function GetLastRecordWithWhere(ByVal TableName As String, ByVal id As String, ByVal Where As String, Optional ByVal FieldToReturn As String = "*") As String
        Dim DR As MySqlDataReader = Sql_Query_DataReader(String.Format("select {0} from {1} where {2} order by {3} desc limit 1", FieldToReturn, TableName, Where, id))
        Dim RetVal As String = ""
        Try
            DR.Read()
            RetVal = DR(0).ToString()
            DR.Close()
        Catch ex As Exception
            DR.Close()
        End Try
        Return RetVal
    End Function
    Public Function GetLastRecordShuffle(ByVal TableName As String, Optional ByVal FieldToReturn As String = "*", Optional ByVal OrderBy As String = "rand()") As String
        Dim DR As MySqlDataReader = Sql_Query_DataReader(String.Format("select {0} from {1} order by {2} desc limit 1", FieldToReturn, TableName, OrderBy))
        Dim RetVal As String = ""
        Try
            DR.Read()
            RetVal = DR(0).ToString()
            DR.Close()
        Catch ex As Exception
            DR.Close()
        End Try
        Return RetVal
    End Function
    Public Function GetLastRecordShuffleWhitWhere(ByVal TableName As String, ByVal Where As String, Optional ByVal FieldToReturn As String = "*", Optional ByVal OrderBy As String = "rand()") As String
        Dim DR As MySqlDataReader = Sql_Query_DataReader(String.Format("select {0} from {1} where {2} order by {3} desc limit 1", FieldToReturn, TableName, Where, OrderBy))
        Dim RetVal As String = ""
        Try
            DR.Read()
            RetVal = DR(0).ToString()
            DR.Close()
        Catch ex As Exception
            DR.Close()
        End Try
        Return RetVal
    End Function

    Public Function GetLastRecordWithDirectSQL(ByVal SqlStr As String) As String
        Dim DR As MySqlDataReader = Sql_Query_DataReader(SqlStr)
        Dim RetVal As String = ""
        Try
            DR.Read()
            RetVal = DR(0).ToString()
            DR.Close()
        Catch ex As Exception
            DR.Close()
        End Try
        Return RetVal
    End Function

    Public Function GetDataBaseList() As StringCollection
        Dim RetVal As New StringCollection()
        Dim StrSql As String = "SHOW DATABASES"
        Dim DR As MySqlDataReader = Sql_Query_DataReader(StrSql)
        While DR.Read()
            RetVal.Add(DR(0).ToString())
        End While
        DR.Close()
        Return RetVal
    End Function

    Public Function GetTableNameList() As StringCollection
        Dim RetVal As New StringCollection()
        Dim StrSql As String = "SHOW TABLES"
        Dim DR As MySqlDataReader = Sql_Query_DataReader(StrSql)
        While DR.Read()
            RetVal.Add(DR(0).ToString())
        End While
        DR.Close()
        Return RetVal
    End Function

End Class
