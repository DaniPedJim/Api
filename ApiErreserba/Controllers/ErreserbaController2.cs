using ApiErreserba.Models;
using ApiErreserba.Responses;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiErreserba.Controllers
{
    internal class ErreserbaController
    {
        [HttpPost("select")]
        public async Task<ActionResult<ApiResponse<List<Erreserba>>>> Bistaratu()
        {
            try
            {
                using var con = new MySqlConnection(_connectionString);
                await con.OpenAsync();

                using var cmd = new MySqlCommand(
                    "SELECT id, pista_id, bazkidea_id, data, ordua " +
                    "FROM kiroldegia.erreserba WHERE desgaituta = 0",
                    con);

                var lista = new List<Erreserba>();
                using var rd = await cmd.ExecuteReaderAsync();

                while (await rd.ReadAsync())
                {
                    lista.Add(new Erreserba
                    {
                        Id = rd.GetInt32("id"),
                        Pista_id = rd.GetInt32("pista_id"),
                        Bazkidea_id = rd.GetInt32("bazkidea_id"),
                        Data = DateOnly.FromDateTime(rd.GetDateTime("data")),
                        Ordua = TimeOnly.FromTimeSpan(rd.GetTimeSpan("ordua"))
                    });
                }

                return Ok(ApiResponse<List<Erreserba>>.Ok(lista, 200, "Erreserbak bistaratuta."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiError.Create(500, "datu_basearen_errorea",
                    "Errorea: " + ex.Message));
            }
        }

    }
}
