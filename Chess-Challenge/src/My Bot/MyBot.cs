using ChessChallenge.API;
using System.Collections.Generic;
public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        // Implement a tree search
        // Hash table of bitboards to allow for pruning
        // Evaluation function

        Move[] moves = board.GetLegalMoves();
        Move bestMove = Move.NullMove;
        bool isWhite = board.IsWhiteToMove;
        float? bestScore = null;
        visited = new HashSet<ulong>();
        foreach (Move legalMove in moves)
        {
            board.MakeMove(legalMove);
            float score = (isWhite ? 1 : -1) * RecursiveSearch(board, 3);
            System.Console.WriteLine(
                ChessChallenge.Chess.MoveUtility.GetMoveNameSAN( legalMove, board )+ ": " + score
            );
            if (bestScore == null || score > bestScore)
            {
                bestMove = legalMove;
                bestScore = score;
            }
            board.UndoMove(legalMove);
        }
        return bestMove;
    }


    private HashSet<ulong> visited;
    public float RecursiveSearch(Board board, int depth)
    {
        //
        if (visited.Contains(board.ZobristKey) || board.IsInCheckmate())
        {
            return (board.IsWhiteToMove ? 1 : -1) * 1000;
        }
        else visited.Add(board.ZobristKey);

        if (depth == 0) return Evaluate(board);

        float max = float.MinValue;
        foreach (Move legalMove in board.GetLegalMoves())
        {
            board.MakeMove(legalMove);
            float score = -RecursiveSearch(board, depth - 1);
            if (score > max)
            {
                max = score;
            }
            board.UndoMove(legalMove);
        }

        return max;
    }

    public float Evaluate(Board board)
    {
        PieceList[] pieceList = board.GetAllPieceLists();

        return
            ((float)200 * (pieceList[5].Count - pieceList[11].Count) +
            9 * (pieceList[4].Count - pieceList[10].Count) +
            5 * (pieceList[3].Count - pieceList[9].Count) +
            3 * (pieceList[2].Count + pieceList[1].Count - pieceList[8].Count - pieceList[7].Count) +
            pieceList[0].Count - pieceList[6].Count -
            ((float)(0.5 * (getNumBadPawns(board,true) - getNumBadPawns(board,false))))) * (board.IsWhiteToMove ? 1 : -1);
    }

    public int getNumBadPawns(Board board, bool isWhite)
    {
        int numBadPawns = 0;
        ulong enemyBitboard = board.GetPieceBitboard(PieceType.Pawn, !isWhite);

        int[] files = { 0, 0, 0, 0, 0, 0, 0, 0 };
        PieceList pawns = board.GetPieceList(PieceType.Pawn, isWhite);

        foreach (Piece pawn in pawns)
        {
            files[pawn.Square.File]++;

            bool isBlocked = true;
            for (int i = -1; i <= 1; i++)
            {
                bool enemyBitboardIsSet = BitboardHelper.SquareIsSet(
                    enemyBitboard,
                    new Square(pawn.Square.File + i, pawn.Square.Rank + (isWhite ? 1 : -1))
                );
                if (
                    pawn.Square.File + i >= 0 &&
                    pawn.Square.File + i <= 7 &&
                    i == 0 != enemyBitboardIsSet
                )
                {
                    isBlocked = false;
                    break;
                }
            }
            if (isBlocked) numBadPawns++;
        }

        return numBadPawns;
        // Doubled

        // Blocked

        // Isolated
    }
}