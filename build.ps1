param
(
    $config = 'Release'
)

dotnet tool install -g Cake.Tool

dotnet cake build.cake
