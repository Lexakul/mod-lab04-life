using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using cli_life;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Text.Json;
namespace NET
{
    [TestClass]
     public class CellTests
    {
        Board board;
        //Board board;
        [TestMethod]
        public void loadboardcheck()
        {
            int x = 0;
            int y = 0;
            int z = 0;
            Board board= new Board(
                width: 50,
                height: 20,
                cellSize: 1,
                liveDensity: 0.5);
            x = board.Width;
            y = board.Height;
            z = board.CellSize;
            Assert.AreEqual(board.Width,x);
            Assert.AreEqual(board.Height,y);
            Assert.AreEqual(board.CellSize,z); 
        }

        [TestMethod]
        public void TestAliveCell()
        {
            var cell = new Cell { IsAliveNext = true };
            cell.Advance();
            Assert.IsTrue(cell.IsAlive);
        }

        [TestMethod]
        public void TestDontAliveCell()
        {
            var cell = new Cell { IsAliveNext = false };
            cell.Advance();
            Assert.IsFalse(cell.IsAlive);
        }

        [TestMethod]
        public void loadTest()
        {
            int x = 0;
            int y = 1;
            string jsonString = File.ReadAllText("BoardSettest.json"); 
            int[,] array = JsonConvert.DeserializeObject<int[,]>(jsonString);
            if(array.Length > 0)
            {
                x = 1;
            }
            Assert.AreEqual(y,x);
        }

        [TestMethod]
        public void statusCellLive()
        {
            var cell = new Cell { IsAlive = false, IsAliveNext = true };
            cell.Advance();
            Assert.IsTrue(cell.IsAlive);
        }

    }
    
}
