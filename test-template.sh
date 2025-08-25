#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}🧪 Testing .NET Web API VSA Template${NC}"

# Clean up any previous test
echo -e "\n${YELLOW}🧹 Cleaning up previous tests...${NC}"
rm -rf TestApiWithAuth TestApiNoAuth
dotnet new uninstall MyWebApi.VSA.Template 2>/dev/null

# Install template
echo -e "\n${YELLOW}📦 Installing template...${NC}"
dotnet new install .

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Template installed successfully${NC}"
else
    echo -e "${RED}❌ Template installation failed${NC}"
    exit 1
fi

# Test 1: Create project with auth
echo -e "\n${YELLOW}🔐 Testing project WITH authentication...${NC}"
dotnet new webapi-vsa -n TestApiWithAuth --IncludeAuth true --CreateAdminUser true --AdminEmail "admin@test.com" --AdminPassword "Test123!"

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Project with auth created successfully${NC}"
else
    echo -e "${RED}❌ Project with auth creation failed${NC}"
    exit 1
fi

# Test 2: Create project without auth  
echo -e "\n${YELLOW}🚫 Testing project WITHOUT authentication...${NC}"
dotnet new webapi-vsa -n TestApiNoAuth --IncludeAuth false

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Project without auth created successfully${NC}"
else
    echo -e "${RED}❌ Project without auth creation failed${NC}"
    exit 1
fi

# Test 3: Build project with auth
echo -e "\n${YELLOW}🔨 Building project with auth...${NC}"
cd TestApiWithAuth
dotnet build

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Project with auth builds successfully${NC}"
else
    echo -e "${RED}❌ Project with auth build failed${NC}"
    cd ..
    exit 1
fi

cd ..

# Test 4: Build project without auth
echo -e "\n${YELLOW}🔨 Building project without auth...${NC}"  
cd TestApiNoAuth
dotnet build

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Project without auth builds successfully${NC}"
else
    echo -e "${RED}❌ Project without auth build failed${NC}"
    cd ..
    exit 1
fi

cd ..

# Test 5: Check file structure
echo -e "\n${YELLOW}📁 Checking file structure...${NC}"

# Check with auth
if [ -d "TestApiWithAuth/src/TestApiWithAuth/Authentication" ]; then
    echo -e "${GREEN}✅ Authentication folder exists in auth project${NC}"
else
    echo -e "${RED}❌ Authentication folder missing in auth project${NC}"
fi

# Check auth config file
if [ -f "TestApiWithAuth/src/TestApiWithAuth/appsettings.auth.json" ]; then
    echo -e "${GREEN}✅ appsettings.auth.json exists in auth project${NC}"
else
    echo -e "${RED}❌ appsettings.auth.json missing in auth project${NC}"
fi

# Check without auth  
if [ ! -d "TestApiNoAuth/src/TestApiNoAuth/Authentication" ]; then
    echo -e "${GREEN}✅ Authentication folder correctly excluded in no-auth project${NC}"
else
    echo -e "${RED}❌ Authentication folder should not exist in no-auth project${NC}"
fi

# Check auth config file exclusion
if [ ! -f "TestApiNoAuth/src/TestApiNoAuth/appsettings.auth.json" ]; then
    echo -e "${GREEN}✅ appsettings.auth.json correctly excluded in no-auth project${NC}"
else
    echo -e "${RED}❌ appsettings.auth.json should not exist in no-auth project${NC}"
fi

# Test 6: Test Docker Compose (optional)
echo -e "\n${YELLOW}🐳 Testing Docker Compose (optional)...${NC}"
cd TestApiWithAuth
if command -v docker-compose &> /dev/null; then
    echo "Testing docker-compose build..."
    docker-compose build
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Docker compose builds successfully${NC}"
    else
        echo -e "${YELLOW}⚠️ Docker compose build failed (but this might be expected)${NC}"
    fi
else
    echo -e "${YELLOW}⚠️ Docker compose not available, skipping${NC}"
fi

cd ..

# Test 7: Check specific files exist
echo -e "\n${YELLOW}📄 Checking key files exist...${NC}"

files_to_check=(
    "TestApiWithAuth/src/TestApiWithAuth/Program.cs"
    "TestApiWithAuth/src/TestApiWithAuth/ConfigureServices.cs"
    "TestApiWithAuth/src/TestApiWithAuth/Endpoints.cs"
    "TestApiWithAuth/src/TestApiWithAuth/Authentication/Endpoints/Login.cs"
    "TestApiWithAuth/src/TestApiWithAuth/Authentication/Endpoints/Register.cs"
    "TestApiWithAuth/docker-compose.yml"
    "TestApiNoAuth/src/TestApiNoAuth/Program.cs"
    "TestApiNoAuth/src/TestApiNoAuth/ConfigureServices.cs"
)

all_files_exist=true
for file in "${files_to_check[@]}"; do
    if [ -f "$file" ]; then
        echo -e "${GREEN}✅ $file exists${NC}"
    else
        echo -e "${RED}❌ $file missing${NC}"
        all_files_exist=false
    fi
done

if [ "$all_files_exist" = true ]; then
    echo -e "\n${GREEN}🎉 All key files exist!${NC}"
else
    echo -e "\n${RED}❌ Some files are missing${NC}"
fi

echo -e "\n${GREEN}🎉 Template testing completed!${NC}"

# Cleanup option
read -p "Do you want to clean up test projects? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo -e "${YELLOW}🧹 Cleaning up...${NC}"
    rm -rf TestApiWithAuth TestApiNoAuth
    dotnet new uninstall MyWebApi.VSA.Template
    echo -e "${GREEN}✅ Cleanup completed${NC}"
else
    echo -e "${YELLOW}Test projects kept for manual inspection${NC}"
fi