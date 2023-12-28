using Microsoft.AspNetCore.Mvc;
using DomainDefinitions.Data;
using DomainDefinitions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using DomainDefinitions.AuxStaticCode;
using System.Xml;

namespace WebApplication1.Infrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RetrieveBookData
    {
        private readonly AppDbContext _appDbContext;

        public RetrieveBookData(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }


        //[HttpPut]
        //public async Task<IActionResult> AddBookData(BookData bookData)
        //{
        //    _appDbContext.bookDatas.Add(bookData);
        //    await _appDbContext.SaveChangesAsync();

        //    return new Microsoft.AspNetCore.Mvc.OkObjectResult(bookData);
        //}

        //[HttpDelete]
        //public async Task<IActionResult> RemoveBookData(BookData bookData)
        //{
        //    _appDbContext.bookDatas.Remove(bookData);
        //    await _appDbContext.SaveChangesAsync();

        //    return new Microsoft.AspNetCore.Mvc.OkObjectResult(bookData);
        //}

        [HttpPost]
        public async Task<IActionResult> GetAll(RetrieveBookDataObject retrieveBookData)
        {
            if (retrieveBookData.LimitRows < AuxStaticCode.minReturnRows)
            {
                return new BadRequestObjectResult($"Error, Param: LimitRows - [{retrieveBookData.LimitRows}] is smaller than [{AuxStaticCode.minReturnRows}]. " +
                    $"Please request the valid params and send a valid value for limitRows.");
            }

            if (retrieveBookData.LimitRows > AuxStaticCode.maxReturnRows)
            {
                return new BadRequestObjectResult($"Error, Param: LimitRows - [{retrieveBookData.LimitRows}] is bigger than [{AuxStaticCode.maxReturnRows}]. " +
                    $"Please request the valid params and send a valid value for limitRows.");
            }

            foreach (var sourceName in retrieveBookData.SourceNames)
            {
                if (!AuxStaticCode.validExchanges.Contains(sourceName))
                {
                    return new BadRequestObjectResult($"Error, Param: SourceNames - [{sourceName}] is not a valid SourceName. " +
                        $"Please request the valid params and send a valid value for SourceNames.");
                }
            }

            foreach (var cryptoPair in retrieveBookData.CryptoPairs)
            {
                if (!AuxStaticCode.validCryptoPairs.Contains(cryptoPair))
                {
                    return new BadRequestObjectResult($"Error, Param: CryptoPairs - [{cryptoPair}] is not a valid CryptoPairs. " +
                        $"Please request the valid params and send a valid value for CryptoPairs.");
                }
            }


            bool hasSourceNames = false;
            bool hasCryptoPairs = false;
            List<BookData> bookDatas;

            if (retrieveBookData.SourceNames != null && retrieveBookData.SourceNames.Count > 0)
            {
                hasSourceNames = true;
            }

            if (retrieveBookData.CryptoPairs != null && retrieveBookData.CryptoPairs.Count > 0)
            {
                hasCryptoPairs = true;
            }

            if (hasSourceNames && hasCryptoPairs)
            {
                bookDatas = await QueryBookDataBySourceNameAndCryptoPair(retrieveBookData.SourceNames, retrieveBookData.CryptoPairs, retrieveBookData.LimitRows);
            }

            else if (hasSourceNames)
            {
                bookDatas = await QueryBookDataBySourceName(retrieveBookData.SourceNames, retrieveBookData.LimitRows);
            }

            else if (hasCryptoPairs)
            {
                bookDatas = await QueryBookDataByCryptoPair(retrieveBookData.SourceNames, retrieveBookData.LimitRows);
            }

            else
            {
                bookDatas = await QueryBookData(retrieveBookData.LimitRows);
            }

            return new OkObjectResult(bookDatas);
        }

        private async Task<List<BookData>> QueryBookData(int limit)
        {
            var bookDataResult = (from bookData in _appDbContext.bookDatas
                                  select bookData).Take(limit);

            return await bookDataResult.ToListAsync();
        }

        private async Task<List<BookData>> QueryBookDataBySourceName(List<string> sourceNames, int limit)
        {
            var bookDataResult = (from bookData in _appDbContext.bookDatas
                                        where sourceNames.Contains(bookData.SourceName)
                                        select bookData).Take(limit);

            return await bookDataResult.ToListAsync();
        }

        private async Task<List<BookData>> QueryBookDataByCryptoPair(List<string> cryptoPairs, int limit)
        {
            var bookDataResult = (from bookData in _appDbContext.bookDatas
                                        where cryptoPairs.Contains(bookData.CryptoPair)
                                        select bookData).Take(limit);

            return await bookDataResult.ToListAsync();
        }

        private async Task<List<BookData>> QueryBookDataBySourceNameAndCryptoPair(List<string> sourceNames, List<string> cryptoPairs, int limit)
        {
            var allData = _appDbContext.bookDatas.ToList();

            var bookDataResult = (from bookData in _appDbContext.bookDatas
                                        where (sourceNames.Contains(bookData.SourceName) && cryptoPairs.Contains(bookData.CryptoPair))
                                        select bookData).Take(limit);

            return await bookDataResult.ToListAsync();
        }
    }
}
