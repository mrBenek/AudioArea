Open solution:
1. Install SQL Server 2022
2. Install Microsoft SQL Server Management Studio
3. Change connection string in Audio.cs file

4. Open terminal in Visual Studio View->Terminal
   Install dotnef-ef:          dotnet tool install --global dotnet-ef
   Change folder to project:   cd .\WebScraper\
   Copy migration script:      dotnet-ef migrations script

5. Open Microsoft SQL Server Management Studio and run copied script
6. Run project WebScraper with LoadFromJson configuration to fill database