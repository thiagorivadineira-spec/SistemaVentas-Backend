using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaVentasApi.Data;
using System.Globalization;

namespace SistemaVentasApi.Services
{
    public interface IReporteService
    {
        /// <summary>Genera el PDF. tipo: "stock", "entradas" o "salidas".</summary>
        Task<byte[]> GenerarAsync(string tipo, DateTime? desde, DateTime? hasta);

        string NormalizarTipo(string? tipo);
    }

    public class ReporteService : IReporteService
    {
        public const string Stock = "stock";
        public const string Entradas = "entradas";
        public const string Salidas = "salidas";

        private static readonly CultureInfo Cultura = ObtenerCultura();

        private readonly DataContext _context;

        public ReporteService(DataContext context)
        {
            _context = context;
        }

        public string NormalizarTipo(string? tipo)
        {
            var t = (tipo ?? string.Empty).Trim().ToLowerInvariant();

            return t is Stock or Entradas or Salidas
                ? t
                : throw new ArgumentException("Tipo de reporte inválido. Use: stock, entradas o salidas.");
        }

        public async Task<byte[]> GenerarAsync(string tipo, DateTime? desde, DateTime? hasta)
        {
            switch (NormalizarTipo(tipo))
            {
                case Stock:
                    return await GenerarStockAsync();
                case Entradas:
                    return await GenerarMovimientosAsync(true, desde, hasta);
                default:
                    return await GenerarMovimientosAsync(false, desde, hasta);
            }
        }

        // ---------------- STOCK ----------------
        private async Task<byte[]> GenerarStockAsync()
        {
            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    p.Nombre,
                    Categoria = p.CategoriaProducto.Nombre,
                    p.StockActual,
                    p.PuntoReposicion,
                    p.StockMaximo,
                    p.PrecioVenta
                })
                .ToListAsync();

            var valorTotal = productos.Sum(p => p.StockActual * p.PrecioVenta);
            var aReponer = productos.Count(p => p.StockActual <= p.PuntoReposicion);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page, "Reporte de stock", $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(8);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(4);   // Producto
                                c.RelativeColumn(3);   // Categoría
                                c.RelativeColumn(1.5f); // Stock
                                c.RelativeColumn(1.5f); // Reposición
                                c.RelativeColumn(1.5f); // Máximo
                                c.RelativeColumn(2.5f); // Precio
                                c.RelativeColumn(2);   // Estado
                            });

                            table.Header(h =>
                            {
                                h.Cell().Element(EncabezadoCelda).Text("Producto");
                                h.Cell().Element(EncabezadoCelda).Text("Categoría");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Stock");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Reposición");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Máximo");
                                h.Cell().Element(EncabezadoCelda).AlignRight().Text("Precio venta");
                                h.Cell().Element(EncabezadoCelda).Text("Estado");
                            });

                            foreach (var p in productos)
                            {
                                var reponer = p.StockActual <= p.PuntoReposicion;

                                table.Cell().Element(Celda).Text(p.Nombre);
                                table.Cell().Element(Celda).Text(p.Categoria);
                                table.Cell().Element(Celda).AlignRight().Text(p.StockActual.ToString(Cultura));
                                table.Cell().Element(Celda).AlignRight().Text(p.PuntoReposicion.ToString(Cultura));
                                table.Cell().Element(Celda).AlignRight().Text(p.StockMaximo.ToString(Cultura));
                                table.Cell().Element(Celda).AlignRight().Text(Dinero(p.PrecioVenta));
                                table.Cell().Element(Celda)
                                    .Text(reponer ? "REPONER" : "OK")
                                    .FontColor(reponer ? Colors.Red.Medium : Colors.Green.Darken2);
                            }
                        });

                        col.Item().PaddingTop(6).Text(t =>
                        {
                            t.Span($"Productos: {productos.Count}   |   A reponer: {aReponer}   |   Valor del stock a precio de venta: ").SemiBold();
                            t.Span(Dinero(valorTotal)).SemiBold();
                        });
                    });
                });
            }).GeneratePdf();
        }

        // ------------- ENTRADAS / SALIDAS -------------
        private async Task<byte[]> GenerarMovimientosAsync(bool entradas, DateTime? desde, DateTime? hasta)
        {
            var hastaFecha = (hasta ?? DateTime.Today).Date;
            var desdeFecha = (desde ?? hastaFecha.AddDays(-30)).Date;

            if (desdeFecha > hastaFecha)
            {
                throw new ArgumentException("La fecha 'desde' no puede ser posterior a 'hasta'.");
            }

            var hastaExclusiva = hastaFecha.AddDays(1);

            List<LineaMovimiento> lineas;

            if (entradas)
            {
                lineas = await _context.Compras
                    .AsNoTracking()
                    .Where(c => c.Fecha >= desdeFecha && c.Fecha < hastaExclusiva)
                    .OrderBy(c => c.Fecha)
                    .SelectMany(c => c.Detalles.Select(d => new LineaMovimiento
                    {
                        Fecha = c.Fecha,
                        Tercero = c.Proveedor.RazonSocial,
                        Usuario = c.Usuario.Nombre,
                        Producto = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    }))
                    .ToListAsync();
            }
            else
            {
                lineas = await _context.Ventas
                    .AsNoTracking()
                    .Where(v => v.Fecha >= desdeFecha && v.Fecha < hastaExclusiva)
                    .OrderBy(v => v.Fecha)
                    .SelectMany(v => v.Detalles.Select(d => new LineaMovimiento
                    {
                        Fecha = v.Fecha,
                        Tercero = v.Cliente.Nombre,
                        Usuario = v.Usuario.Nombre,
                        Producto = d.Producto.Nombre,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario
                    }))
                    .ToListAsync();
            }

            var titulo = entradas ? "Reporte de entradas de mercadería" : "Reporte de salidas (ventas)";
            var terceroTitulo = entradas ? "Proveedor" : "Cliente";
            var subtitulo = $"Período: {desdeFecha:dd/MM/yyyy} al {hastaFecha:dd/MM/yyyy}   |   Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";

            var totalUnidades = lineas.Sum(l => l.Cantidad);
            var totalImporte = lineas.Sum(l => l.Cantidad * l.PrecioUnitario);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page, titulo, subtitulo);

                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        col.Spacing(8);

                        if (lineas.Count == 0)
                        {
                            col.Item().Text("No hay movimientos en el período seleccionado.");
                        }
                        else
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(2.5f); // Fecha
                                    c.RelativeColumn(3);    // Proveedor / Cliente
                                    c.RelativeColumn(2.5f); // Usuario
                                    c.RelativeColumn(4);    // Producto
                                    c.RelativeColumn(1.3f); // Cantidad
                                    c.RelativeColumn(2.5f); // Precio unitario
                                    c.RelativeColumn(2.7f); // Subtotal
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Element(EncabezadoCelda).Text("Fecha");
                                    h.Cell().Element(EncabezadoCelda).Text(terceroTitulo);
                                    h.Cell().Element(EncabezadoCelda).Text("Registró");
                                    h.Cell().Element(EncabezadoCelda).Text("Producto");
                                    h.Cell().Element(EncabezadoCelda).AlignRight().Text("Cant.");
                                    h.Cell().Element(EncabezadoCelda).AlignRight().Text("P. unitario");
                                    h.Cell().Element(EncabezadoCelda).AlignRight().Text("Subtotal");
                                });

                                foreach (var l in lineas)
                                {
                                    table.Cell().Element(Celda).Text(l.Fecha.ToString("dd/MM/yyyy HH:mm"));
                                    table.Cell().Element(Celda).Text(l.Tercero);
                                    table.Cell().Element(Celda).Text(l.Usuario);
                                    table.Cell().Element(Celda).Text(l.Producto);
                                    table.Cell().Element(Celda).AlignRight().Text(l.Cantidad.ToString(Cultura));
                                    table.Cell().Element(Celda).AlignRight().Text(Dinero(l.PrecioUnitario));
                                    table.Cell().Element(Celda).AlignRight().Text(Dinero(l.Cantidad * l.PrecioUnitario));
                                }
                            });

                            col.Item().PaddingTop(6).Text(t =>
                            {
                                t.Span($"Unidades: {totalUnidades}   |   Total: ").SemiBold();
                                t.Span(Dinero(totalImporte)).SemiBold();
                            });
                        }
                    });
                });
            }).GeneratePdf();
        }

        // ------------- Utilidades de formato -------------
        private static void ConfigurarPagina(PageDescriptor page, string titulo, string subtitulo)
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Column(col =>
            {
                col.Item().Text(titulo).FontSize(18).SemiBold().FontColor(Colors.Blue.Darken3);
                col.Item().Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            });

            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("Página ");
                t.CurrentPageNumber();
                t.Span(" de ");
                t.TotalPages();
            });
        }

        private static IContainer EncabezadoCelda(IContainer c) =>
            c.Background(Colors.Grey.Lighten3)
             .PaddingVertical(4)
             .PaddingHorizontal(3)
             .DefaultTextStyle(x => x.SemiBold());

        private static IContainer Celda(IContainer c) =>
            c.BorderBottom(1)
             .BorderColor(Colors.Grey.Lighten2)
             .PaddingVertical(3)
             .PaddingHorizontal(3);

        private static string Dinero(decimal valor) =>
            "$ " + valor.ToString("N2", Cultura);

        private static CultureInfo ObtenerCultura()
        {
            try
            {
                return CultureInfo.GetCultureInfo("es-AR");
            }
            catch (CultureNotFoundException)
            {
                // Servidores con globalización invariante (p. ej. algunos contenedores)
                return CultureInfo.InvariantCulture;
            }
        }

        private class LineaMovimiento
        {
            public DateTime Fecha { get; set; }
            public string Tercero { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string Producto { get; set; } = string.Empty;
            public int Cantidad { get; set; }
            public decimal PrecioUnitario { get; set; }
        }
    }
}
