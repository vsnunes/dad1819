' Generated by TinyPG v1.3 available at www.codeproject.com

Imports System
Imports System.Collections.Generic


Namespace TinyPG
#Region "Parser"

    Partial Public Class Parser 
        Private m_scanner As Scanner
        Private m_tree As ParseTree

        Public Sub New(ByVal scanner As Scanner)
            m_scanner = scanner
        End Sub


    Public Function Parse(ByVal input As String) As ParseTree
            m_tree = New ParseTree()
            Return Parse(input, m_tree)
        End Function

        Public Function Parse(ByVal input As String, ByVal tree As ParseTree) As ParseTree
            m_scanner.Init(input)

            m_tree = tree
            ParseStart(m_tree)
            m_tree.Skipped = m_scanner.Skipped

            Return m_tree
        End Function

        Private Sub ParseStart(ByVal parent As ParseNode)
            Dim tok As Token
            Dim n As ParseNode
            Dim node As ParseNode = parent.CreateNode(m_scanner.GetToken(TokenType.Start), "Start")
            parent.Nodes.Add(node)


            
            tok = m_scanner.LookAhead(TokenType.NUMBER, TokenType.BROPEN)
            If tok.Type = TokenType.NUMBER Or tok.Type = TokenType.BROPEN Then
                ParseAddExpr(node)
            End If

            
            tok = m_scanner.Scan(TokenType.EOF)
            n = node.CreateNode(tok, tok.ToString() )
            node.Token.UpdateRange(tok)
            node.Nodes.Add(n)
            If tok.Type <> TokenType.EOF Then
                m_tree.Errors.Add(New ParseError("Unexpected token '" + tok.Text.Replace("\n", "") + "' found. Expected " + TokenType.EOF.ToString(), &H1001, 0, tok.StartPos, tok.StartPos, tok.EndPos - tok.StartPos))
                Return

            End If


            parent.Token.UpdateRange(node.Token)
        End Sub

        Private Sub ParseAddExpr(ByVal parent As ParseNode)
            Dim tok As Token
            Dim n As ParseNode
            Dim node As ParseNode = parent.CreateNode(m_scanner.GetToken(TokenType.AddExpr), "AddExpr")
            parent.Nodes.Add(node)


            
            ParseMultExpr(node)

            
            tok = m_scanner.LookAhead(TokenType.PLUSMINUS)
            While tok.Type = TokenType.PLUSMINUS

                
                tok = m_scanner.Scan(TokenType.PLUSMINUS)
                n = node.CreateNode(tok, tok.ToString() )
                node.Token.UpdateRange(tok)
                node.Nodes.Add(n)
                If tok.Type <> TokenType.PLUSMINUS Then
                    m_tree.Errors.Add(New ParseError("Unexpected token '" + tok.Text.Replace("\n", "") + "' found. Expected " + TokenType.PLUSMINUS.ToString(), &H1001, 0, tok.StartPos, tok.StartPos, tok.EndPos - tok.StartPos))
                    Return

                End If


                
                ParseMultExpr(node)
            tok = m_scanner.LookAhead(TokenType.PLUSMINUS)
            End While

            parent.Token.UpdateRange(node.Token)
        End Sub

        Private Sub ParseMultExpr(ByVal parent As ParseNode)
            Dim tok As Token
            Dim n As ParseNode
            Dim node As ParseNode = parent.CreateNode(m_scanner.GetToken(TokenType.MultExpr), "MultExpr")
            parent.Nodes.Add(node)


            
            ParseAtom(node)

            
            tok = m_scanner.LookAhead(TokenType.MULTDIV)
            While tok.Type = TokenType.MULTDIV

                
                tok = m_scanner.Scan(TokenType.MULTDIV)
                n = node.CreateNode(tok, tok.ToString() )
                node.Token.UpdateRange(tok)
                node.Nodes.Add(n)
                If tok.Type <> TokenType.MULTDIV Then
                    m_tree.Errors.Add(New ParseError("Unexpected token '" + tok.Text.Replace("\n", "") + "' found. Expected " + TokenType.MULTDIV.ToString(), &H1001, 0, tok.StartPos, tok.StartPos, tok.EndPos - tok.StartPos))
                    Return

                End If


                
                ParseAtom(node)
            tok = m_scanner.LookAhead(TokenType.MULTDIV)
            End While

            parent.Token.UpdateRange(node.Token)
        End Sub

        Private Sub ParseAtom(ByVal parent As ParseNode)
            Dim tok As Token
            Dim n As ParseNode
            Dim node As ParseNode = parent.CreateNode(m_scanner.GetToken(TokenType.Atom), "Atom")
            parent.Nodes.Add(node)

            tok = m_scanner.LookAhead(TokenType.NUMBER, TokenType.BROPEN)
            Select Case tok.Type
            
                Case TokenType.NUMBER
                    tok = m_scanner.Scan(TokenType.NUMBER)
                    n = node.CreateNode(tok, tok.ToString() )
                    node.Token.UpdateRange(tok)
                    node.Nodes.Add(n)
                    If tok.Type <> TokenType.NUMBER Then
                        m_tree.Errors.Add(New ParseError("Unexpected token '" + tok.Text.Replace("\n", "") + "' found. Expected " + TokenType.NUMBER.ToString(), &H1001, 0, tok.StartPos, tok.StartPos, tok.EndPos - tok.StartPos))
                        Return

                    End If

                Case TokenType.BROPEN

                    
                    tok = m_scanner.Scan(TokenType.BROPEN)
                    n = node.CreateNode(tok, tok.ToString() )
                    node.Token.UpdateRange(tok)
                    node.Nodes.Add(n)
                    If tok.Type <> TokenType.BROPEN Then
                        m_tree.Errors.Add(New ParseError("Unexpected token '" + tok.Text.Replace("\n", "") + "' found. Expected " + TokenType.BROPEN.ToString(), &H1001, 0, tok.StartPos, tok.StartPos, tok.EndPos - tok.StartPos))
                        Return

                    End If


                    
                    ParseAddExpr(node)

                    
                    tok = m_scanner.Scan(TokenType.BRCLOSE)
                    n = node.CreateNode(tok, tok.ToString() )
                    node.Token.UpdateRange(tok)
                    node.Nodes.Add(n)
                    If tok.Type <> TokenType.BRCLOSE Then
                        m_tree.Errors.Add(New ParseError("Unexpected token '" + tok.Text.Replace("\n", "") + "' found. Expected " + TokenType.BRCLOSE.ToString(), &H1001, 0, tok.StartPos, tok.StartPos, tok.EndPos - tok.StartPos))
                        Return

                    End If

                Case Else
                    m_tree.Errors.Add(new ParseError("Unexpected token '" + tok.Text.Replace("\n", "") + "' found.", &H0002, 0, tok.StartPos, tok.StartPos, tok.EndPos - tok.StartPos))
                    Exit Select
            End Select

            parent.Token.UpdateRange(node.Token)
        End Sub


    End Class
#End Region
End Namespace
