namespace CalculadoraTaller;

/// <summary>Recetas originales tomadas de TABLA BARNIZ BREUER.xlsx y TABLA BARNIZ IDEMO.xlsx.</summary>
static class DefaultData
{
    static FinishItem I(string stage, string code, string name, double qty, double yld, string app = "", string unit = "LT") =>
        new() { Stage = stage, Code = code, Name = name, Qty = qty, Yield = yld, Application = app, Unit = unit };

    static IEnumerable<FinishItem> Polyform(string stage, double yld = 1) => new[]
    {
        I(stage, "12230186", "POLYFORM BARNIZ 11000 BRILLANTE", 0.375, yld),
        I(stage, "12230198", "SOLVENTE P/ POLIURETANO POLYFORM (COMEX)", 0.35, yld),
        I(stage, "12230110", "CATALIZADOR ENDURESAYER CRISTAL (UC-2000.50)", 0.25, yld),
    };

    static IEnumerable<FinishItem> Fondo(string stage, double yld) => new[]
    {
        I(stage, "12230032", "FONDO DE POLIURETANO (UF-1200)", 0.375, yld),
        I(stage, "12230013", "DILUYENTE P/POLIURETANO (D-0500)", 0.35, yld),
        I(stage, "12230015", "ENDURESAYER (UC-0282) CATALIZADOR P/POLIURETANO", 0.25, yld),
    };

    static IEnumerable<FinishItem> AcabadoMate() => new[]
    {
        I("ACABADO MATE", "12230007", "BARNIZ MATE DE POLIURETANO (UM-1015)", 0.375, 1),
        I("ACABADO MATE", "12230013", "DILUYENTE P/POLIURETANO (D-0500)", 0.35, 1),
        I("ACABADO MATE", "12230110", "CATALIZADOR ENDURESAYER CRISTAL (UC-2000.50)", 0.25, 1),
    };

    static IEnumerable<FinishItem> AcabadoBrillante() => new[]
    {
        I("ACABADO BRILLANTE", "12230135", "BARNIZ DE POLIURETANO (UB-5000 SAYER LACK)", 0.375, 1),
        I("ACABADO BRILLANTE", "12230013", "DILUYENTE P/POLIURETANO (D-0500)", 0.35, 1),
        I("ACABADO BRILLANTE", "12230110", "CATALIZADOR ENDURESAYER CRISTAL (UC-2000.50)", 0.25, 1),
    };

    static IEnumerable<FinishItem> Laca() => new[]
    {
        I("LACA AUTOMOTIVA", "**", "LACA AUTOMOTIVA", 0.5, 4),
        I("LACA AUTOMOTIVA", "", "SOLVENTE D-8000.50", 0.5, 4),
    };

    static FinishSystem S(string name, string note, params IEnumerable<FinishItem>[] parts) =>
        new() { Name = name, Note = note, Items = parts.SelectMany(p => p).Select(x => x.Clone()).ToList() };

    const string NotaPU = "ES APLICABLE EN TODO TIPO DE MADERA, TRIPLAY, MDF, ENCHAPADOS, PARA MUEBLES COMO CLOSET, PUERTAS, MESAS. (SOLO DETERMINA SI ES A PORO ABIERTO / PORO CERRADO, TÉRMINO MATE O TÉRMINO BRILLO).";
    const string NotaPolyform = "ES APLICABLE EN TODO TIPO DE MADERA, TRIPLAY, MDF, ENCHAPADOS, PARA CUBIERTAS DE MUEBLES QUE NECESITAN MÁS RESISTENCIA A SU USO. (SOLO DETERMINA SI ES A PORO ABIERTO / PORO CERRADO, TÉRMINO MATE O TÉRMINO BRILLO).";

    public static List<FinishSystem> Breuer() => new()
    {
        S("BARNIZ POLIURETANO MATE PORO CERRADO", NotaPU, Polyform("BASE PORO CERRADO"), Fondo("FONDO PORO CERRADO", 1), AcabadoMate()),
        S("BARNIZ POLIURETANO MATE PORO ABIERTO", NotaPU, Polyform("BASE PORO ABIERTO"), Fondo("FONDO PORO ABIERTO", 1.5), AcabadoMate()),
        S("BARNIZ POLIURETANO BRILLANTE PORO CERRADO", NotaPU, Polyform("BASE PORO CERRADO"), Fondo("FONDO PORO CERRADO", 1), AcabadoBrillante()),
        S("BARNIZ POLIURETANO BRILLANTE PORO ABIERTO", NotaPU, Polyform("BASE PORO ABIERTO"), Fondo("FONDO PORO ABIERTO", 1.5), AcabadoBrillante()),
        S("ACABADO EN POLYFORM MATE PORO CERRADO", NotaPolyform, Polyform("BASE PORO CERRADO"), Fondo("FONDO PORO CERRADO", 1), Polyform("ACABADO")),
        S("ACABADO EN POLYFORM MATE PORO ABIERTO", NotaPolyform, Polyform("BASE PORO ABIERTO"), Fondo("FONDO PORO ABIERTO", 1.5), Polyform("ACABADO")),
        S("ACABADO EN POLYFORM BRILLANTE PORO CERRADO", NotaPolyform, Polyform("BASE PORO CERRADO"), Fondo("FONDO PORO CERRADO", 1), Polyform("ACABADO")),
        S("ACABADO EN POLYFORM BRILLANTE PORO ABIERTO", NotaPolyform, Polyform("BASE PORO ABIERTO"), Fondo("FONDO PORO ABIERTO", 1.5), Polyform("ACABADO")),
        S("ACABADO EN BONA", "ESTE PRODUCTO ES MAYORMENTE APLICADO EN MADERA DE ENCINO. SU APLICACIÓN ES CON BROCHA O TRAPO Y ES PARA MUEBLES COMO BANCOS Y REPISAS.",
            new[] { I("POLIURETANO BASE AGUA", "12230181", "BONA TRAFIC (INCLUYE CATALIZADOR)", 1, 4) }),
        S("SELLADOR RIVER COMEX", "ESTE PRODUCTO SE DILUYE CON THINNER. SU APLICACIÓN ES CON BROCHA Y SE UTILIZA MAYORMENTE EN MUEBLES DE LA LÍNEA AURRERA.",
            new[] { I("RIVER SELLADOR COMEX", "12230119", "RIVER SELLADOR 48% CONCENTRADO (COMEX)", 1, 3) }),
        S("LACA AUTOMOTIVA BRILLANTE PORO CERRADO", "", Fondo("FONDO PORO CERRADO", 1), Laca()),
        S("LACA AUTOMOTIVA BRILLANTE PORO ABIERTO", "", Fondo("FONDO PORO ABIERTO", 1.5), Laca()),
        S("LACA AUTOMOTIVA MATE PORO CERRADO", "", Fondo("FONDO PORO CERRADO", 1), Laca()),
        S("LACA AUTOMOTIVA MATE PORO ABIERTO", "", Fondo("FONDO PORO ABIERTO", 1.5), Laca()),
    };

    public static List<FinishSystem> Idemo() => new()
    {
        S("ACABADO NATURAL", "", new[]
        {
            I("ACABADO NATURAL", "12230119", "RIVER SELLADOR 48%", 0.3, 2.5, "APLICACIÓN A 3 MANOS"),
            I("ACABADO NATURAL", "12230321 / 12230276", "LACA DE NITROCELULOSA 48% MATE / BRILLANTE", 0.65, 2.5, "APLICACIÓN A 3 MANOS"),
        }),
        S("ENTINTADO (agregar al Acabado Natural)", "EN CASO DE REQUERIR ENTINTADO, AL PROCESO DE ACABADO NATURAL AGREGAR LO SIGUIENTE (elige los colores que uses; borra o pon 0 en los que no).", new[]
        {
            I("ENTINTADO", "12230017", "ESMALTE ROJO ÓXIDO COMEX 100", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("ENTINTADO", "12230021", "ESMALTE BLANCO COMEX", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("ENTINTADO", "12230022", "ESMALTE AMARILLO CROMO COMEX 100", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("ENTINTADO", "12230024", "ESMALTE NEGRO COMEX", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("ENTINTADO", "12230236", "ESMALTE MARRÓN COMEX", 0.25, 12, "APLICACIÓN A 1 MANO"),
        }),
        S("POLIURETANO SISTEMA F-218", "", new[]
        {
            I("FONDO", "12230525", "FONDO DE POLIURETANO F-218 TRANSPARENTE", 0.45, 2.5, "APLICACIÓN A 3 MANOS"),
            I("FONDO", "12230526", "CATALIZADOR P/ FONDO POLIURETANO F-218", 0.45, 2.5, "APLICACIÓN A 3 MANOS"),
            I("FONDO", "12230198", "SOLVENTE PARA POLIURETANO", 0.1, 2.5, "APLICACIÓN A 3 MANOS"),
            I("ACABADO", "12230537 / 12230527", "POLYFORM BARNIZ DE POLIURETANO A-218 MATE / BRILLANTE", 0.45, 2.5, "APLICACIÓN A 3 MANOS"),
            I("ACABADO", "12230538 / 12230528", "CATALIZADOR P/ BARNIZ DE POLIURETANO A-218 MATE / BRILLANTE", 0.45, 2.5, "APLICACIÓN A 3 MANOS"),
            I("ACABADO", "12230198", "SOLVENTE PARA POLIURETANO", 0.1, 2.5, "APLICACIÓN A 3 MANOS"),
        }),
        S("LAQUEADO", "SE SOLICITARÁ CÓDIGO PARA EL PRIMER NITRO-ACRÍLICO DE ACUERDO AL PANTONE NECESITADO. EXCEPCIÓN: EN CASO DE REQUERIR UN LAQUEADO EN COLOR NEGRO SE OMITIRÁ EL PRIMER NITRO-ACRÍLICO.", new[]
        {
            I("BASE", "", "PRIMER NITRO-ACRÍLICO*", 0.25, 2.5, "APLICACIÓN A 3 MANOS"),
            I("BASE", "", "LACA AUTOMOTIVA", 0.25, 2.5, "APLICACIÓN A 3 MANOS"),
            I("BASE", "12230321 / 12230276", "LACA DE NITROCELULOSA 48% MATE / BRILLANTE", 0.25, 2.5, "APLICACIÓN A 3 MANOS"),
        }),
        S("LACAS - IGUALADO DE COLOR (agregar al Laqueado)", "EN CASO DE REQUERIR IGUALADO DE COLOR, AL PROCESO DE LAQUEADO AGREGAR LO SIGUIENTE (elige los colores que uses).", new[]
        {
            I("LACAS", "12230047", "LACA AUTOMOTIVA ROJO ÓXIDO COMEX 100", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("LACAS", "12230041", "LACA AUTOMOTIVA BLANCO COMEX", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("LACAS", "12230039", "LACA AUTOMOTIVA AMARILLO CROMO COMEX", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("LACAS", "12230348", "LACA AUTOMOTIVA NEGRO COMEX", 0.25, 12, "APLICACIÓN A 1 MANO"),
            I("LACAS", "", "LACA AUTOMOTIVA ROJO BERMELLÓN COMEX", 0.25, 12, "APLICACIÓN A 1 MANO"),
        }),
        S("BARNIZ MARINO", "", new[]
        {
            I("BARNIZ MARINO", "12230127", "BARNIZ MARINO BRILLANTE TRANSPARENTE HI-900.50", 1, 5, "APLICACIÓN POR CAPA, NORMALMENTE 2 A 3 CAPAS"),
        }),
        S("IMPERMEABILIZANTE", "", new[]
        {
            I("IMPERMEABILIZANTE", "12230036", "IMPERMEABILIZANTE TOP TOTAL (3 AÑOS)", 1, 1.5, "APLICACIÓN A 1 MANO"),
        }),
        S("PINTURA ELECTROSTÁTICA", "Superficie en m² de láminas y perfiles (se puede enviar desde la pestaña Metal).", new[]
        {
            I("PINTURA ELECTROSTÁTICA", "", "PINTURA ELECTROSTÁTICA", 1, 7, "", "KG"),
        }),
        new FinishSystem
        {
            Name = "ADHESIVO SUPERFICIE SÓLIDA", InputUnit = "m lineales",
            Note = "EL ADHESIVO SE CONSIDERA PARA LA UNIÓN DE SS CON SS Y SE MIDE EN METROS LINEALES. Presentación 250 ml rinde 12.5 m; presentación 50 ml rinde 2.5 m (usa la que corresponda).",
            Items =
            {
                I("ADHESIVO", "", "ADHESIVO SUPERFICIE SÓLIDA - PRESENTACIÓN 250 ml", 1, 12.5, "", "PZA"),
                I("ADHESIVO", "", "ADHESIVO SUPERFICIE SÓLIDA - PRESENTACIÓN 50 ml", 1, 2.5, "", "PZA"),
            },
        },
    };

    public static MetalFormats Metal()
    {
        var m = new MetalFormats();
        void Sh(string t, string n, double l, double w) => m.Sheets.Add(new SheetFormat { Type = t, Name = n, Length = l, Width = w });
        void Pr(string t, string n, double a, double b, double len = 6.1) => m.Profiles.Add(new ProfileFormat { Type = t, Name = n, A = a, B = b, Length = len });

        Sh("Lámina", "Lámina 3' x 8' (0.914 x 2.44)", 2.44, 0.914);
        Sh("Lámina", "Lámina 3' x 10' (0.914 x 3.05)", 3.05, 0.914);
        Sh("Lámina", "Lámina 4' x 8' (1.22 x 2.44)", 2.44, 1.22);
        Sh("Lámina", "Lámina 4' x 10' (1.22 x 3.05)", 3.05, 1.22);
        Sh("Lámina", "Lámina 5' x 10' (1.52 x 3.05)", 3.05, 1.52);
        Sh("Tablero", "Tablero 1.22 x 2.44", 2.44, 1.22);
        Sh("Tablero", "Tablero 1.22 x 3.05", 3.05, 1.22);

        Pr("Cuadrado", "PTR 1\" (25.4 x 25.4)", 25.4, 25.4);
        Pr("Cuadrado", "PTR 1 1/4\" (31.75 x 31.75)", 31.75, 31.75);
        Pr("Cuadrado", "PTR 1 1/2\" (38.1 x 38.1)", 38.1, 38.1);
        Pr("Cuadrado", "PTR 2\" (50.8 x 50.8)", 50.8, 50.8);
        Pr("Cuadrado", "PTR 3\" (76.2 x 76.2)", 76.2, 76.2);
        Pr("Cuadrado", "PTR 4\" (101.6 x 101.6)", 101.6, 101.6);
        Pr("Rectangular", "Rectangular 40 x 20 mm", 40, 20, 6);
        Pr("Rectangular", "PTR 1\" x 2\" (25.4 x 50.8)", 25.4, 50.8);
        Pr("Rectangular", "PTR 2\" x 3\" (50.8 x 76.2)", 50.8, 76.2);
        Pr("Rectangular", "PTR 2\" x 4\" (50.8 x 101.6)", 50.8, 101.6);
        Pr("Redondo hueco", "Tubo redondo 3/4\" (19.05)", 19.05, 0, 6);
        Pr("Redondo hueco", "Tubo redondo 1\" (25.4)", 25.4, 0, 6);
        Pr("Redondo hueco", "Tubo redondo 1 1/4\" (31.75)", 31.75, 0, 6);
        Pr("Redondo hueco", "Tubo redondo 1 1/2\" (38.1)", 38.1, 0, 6);
        Pr("Redondo hueco", "Tubo redondo 2\" (50.8)", 50.8, 0, 6);
        Pr("Barra redonda", "Barra redonda 3/8\" (9.53)", 9.525, 0);
        Pr("Barra redonda", "Barra redonda 1/2\" (12.7)", 12.7, 0);
        Pr("Barra redonda", "Barra redonda 5/8\" (15.88)", 15.875, 0);
        Pr("Barra redonda", "Barra redonda 3/4\" (19.05)", 19.05, 0);
        Pr("Barra redonda", "Barra redonda 1\" (25.4)", 25.4, 0);
        Pr("Barra cuadrada", "Barra cuadrada 1/2\" (12.7)", 12.7, 0);
        Pr("Barra cuadrada", "Barra cuadrada 3/4\" (19.05)", 19.05, 0);
        Pr("Barra cuadrada", "Barra cuadrada 1\" (25.4)", 25.4, 0);
        Pr("Solera", "Solera 1\" x 1/8\" (25.4 x 3.18)", 25.4, 3.175);
        Pr("Solera", "Solera 1 1/2\" x 3/16\" (38.1 x 4.76)", 38.1, 4.7625);
        Pr("Solera", "Solera 2\" x 1/4\" (50.8 x 6.35)", 50.8, 6.35);
        Pr("Ángulo", "Ángulo 1\" x 1/8\" (25.4 x 25.4)", 25.4, 25.4);
        Pr("Ángulo", "Ángulo 1 1/2\" x 3/16\" (38.1 x 38.1)", 38.1, 38.1);
        Pr("Ángulo", "Ángulo 2\" x 1/4\" (50.8 x 50.8)", 50.8, 50.8);
        Pr("Canal / Monten", "Monten 4\" (101.6 x 50.8)", 101.6, 50.8);
        return m;
    }
}
