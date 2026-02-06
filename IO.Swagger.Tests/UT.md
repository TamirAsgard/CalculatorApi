# Unit and Integration Test Cases

This document provides a comprehensive overview of all test cases implemented in the test suite, including inputs and expected outputs.

---

## Unit Tests

### 1. PasswordHasher Tests (9 tests)

#### 1.1 Hash_ValidPassword_ReturnsHashedPassword
- **Input**: `"MySecurePassword123!"`
- **Expected Output**: 
  - Non-null/non-empty hashed password
  - Format: `pbkdf2_sha256$<iterations>$<salt>$<hash>`
  - Contains 4 parts separated by `$`

#### 1.2 Hash_EmptyPassword_ThrowsArgumentException
- **Input**: `""`
- **Expected Output**: `ArgumentException` with message "Password must not be empty.*"

#### 1.3 Hash_NullPassword_ThrowsArgumentException
- **Input**: `null`
- **Expected Output**: `ArgumentException`

#### 1.4 Hash_SamePassword_GeneratesDifferentHashes
- **Input**: `"MySecurePassword123!"` (hashed twice)
- **Expected Output**: Two different hash values (unique salts)

#### 1.5 Verify_CorrectPassword_ReturnsTrue
- **Input**: 
  - Password: `"MySecurePassword123!"`
  - Hash: Result of hashing the same password
- **Expected Output**: `true`

#### 1.6 Verify_IncorrectPassword_ReturnsFalse
- **Input**: 
  - Password: `"WrongPassword456!"`
  - Hash: Hash of `"MySecurePassword123!"`
- **Expected Output**: `false`

#### 1.7 Verify_EmptyPassword_ReturnsFalse
- **Input**: 
  - Password: `""`
  - Hash: Valid hash
- **Expected Output**: `false`

#### 1.8 Verify_InvalidHashFormat_ReturnsFalse
- **Input**: 
  - Password: `"MySecurePassword123!"`
  - Hash: `"invalid_hash_format"`
- **Expected Output**: `false`

#### 1.9 Verify_TamperedHash_ReturnsFalse
- **Input**: 
  - Password: `"MySecurePassword123!"`
  - Hash: Valid hash with last 5 characters replaced with "XXXXX"
- **Expected Output**: `false`

---

### 2. CalculatorService Tests (12 tests)

#### 2.1 CalculateAsync_Addition_ReturnsCorrectResult
- **Test Cases**:
  | Operation | Number1 | Number2 | Expected Result |
  |-----------|---------|---------|-----------------|
  | "add"     | 5.0     | 3.0     | 8.0             |
  | "Add"     | 10.5    | 2.5     | 13.0            |
  | "+"       | 100     | 50      | 150             |
- **Expected Output**: `CalculationResponse` with correct result and operation = "add"

#### 2.2 CalculateAsync_Subtraction_ReturnsCorrectResult
- **Test Cases**:
  | Operation   | Number1 | Number2 | Expected Result |
  |-------------|---------|---------|-----------------|
  | "subtract"  | 10.0    | 3.0     | 7.0             |
  | "Subtract"  | 100.5   | 50.5    | 50.0            |
  | "-"         | 5       | 10      | -5              |
- **Expected Output**: `CalculationResponse` with correct result and operation = "subtract"

#### 2.3 CalculateAsync_Multiplication_ReturnsCorrectResult
- **Test Cases**:
  | Operation   | Number1 | Number2 | Expected Result |
  |-------------|---------|---------|-----------------|
  | "multiply"  | 5.0     | 3.0     | 15.0            |
  | "Multiply"  | 10.5    | 2.0     | 21.0            |
  | "*"         | 7       | 6       | 42              |
- **Expected Output**: `CalculationResponse` with correct result and operation = "multiply"

#### 2.4 CalculateAsync_Division_ReturnsCorrectResult
- **Test Cases**:
  | Operation | Number1 | Number2 | Expected Result |
  |-----------|---------|---------|-----------------|
  | "divide"  | 10.0    | 2.0     | 5.0             |
  | "Divide"  | 100.0   | 4.0     | 25.0            |
  | "/"       | 15      | 3       | 5               |
- **Expected Output**: `CalculationResponse` with correct result and operation = "divide"

#### 2.5 CalculateAsync_DivisionByZero_ThrowsInvalidOperationException
- **Input**: 
  - Number1: `10.0`
  - Number2: `0.0`
  - Operation: `"divide"`
- **Expected Output**: `InvalidOperationException` with message "Division by zero is not allowed."

#### 2.6 CalculateAsync_NullRequest_ThrowsArgumentNullException
- **Input**: `null`
- **Expected Output**: `ArgumentNullException`

#### 2.7 CalculateAsync_MissingNumber1_ThrowsArgumentException
- **Input**: 
  - Number1: `null`
  - Number2: `5.0`
- **Expected Output**: `ArgumentException` with message "Number1 is required.*"

#### 2.8 CalculateAsync_MissingNumber2_ThrowsArgumentException
- **Input**: 
  - Number1: `5.0`
  - Number2: `null`
- **Expected Output**: `ArgumentException` with message "Number2 is required.*"

#### 2.9 CalculateAsync_InvalidOperation_ThrowsArgumentException
- **Test Cases**:
  | Operation |
  |-----------|
  | ""        |
  | " "       |
  | null      |
- **Expected Output**: `ArgumentException` with message "Operation header is required.*"

#### 2.10 CalculateAsync_UnsupportedOperation_ThrowsArgumentException
- **Input**: 
  - Number1: `5.0`
  - Number2: `3.0`
  - Operation: `"modulo"`
- **Expected Output**: `ArgumentException` with message "Invalid operation 'modulo'*"

#### 2.11 CalculateAsync_ValidCalculation_SetsTimestamp
- **Input**: 
  - Number1: `5.0`
  - Number2: `3.0`
  - Operation: `"add"`
- **Expected Output**: `CalculationResponse.Timestamp` is between test start and end time (UTC)

---

### 3. AuthService Tests (8 tests)

#### 3.1 LoginOrRegisterAsync_NewUser_CreatesUserAndReturnsToken
- **Input**: 
  - Username: `"newuser"`
  - Password: `"password123"`
- **Mocked Behavior**:
  - User does not exist in repository
  - Password hasher returns `"hashed_password"`
  - Token handler returns token `"jwt_token"` with expiry `1800`
- **Expected Output**: 
  - `TokenResponse` with AccessToken = "jwt_token", ExpiresIn = 1800, TokenType = "Bearer"
  - User created in repository with hashed password

#### 3.2 LoginOrRegisterAsync_ExistingUserCorrectPassword_ReturnsToken
- **Input**: 
  - Username: `"existinguser"`
  - Password: `"password123"`
- **Mocked Behavior**:
  - User exists with matching username
  - Password verification returns `true`
  - Token handler returns token `"jwt_token"` with expiry `1800`
- **Expected Output**: 
  - `TokenResponse` with AccessToken = "jwt_token", ExpiresIn = 1800
  - No new user created

#### 3.3 LoginOrRegisterAsync_ExistingUserWrongPassword_ThrowsInvalidPasswordException
- **Input**: 
  - Username: `"existinguser"`
  - Password: `"wrongpassword"`
- **Mocked Behavior**:
  - User exists
  - Password verification returns `false`
- **Expected Output**: `InvalidPasswordException`

#### 3.4 LoginOrRegisterAsync_ExistingToken_ReusesToken
- **Input**: 
  - Username: `"existinguser"`
  - Password: `"password123"`
- **Mocked Behavior**:
  - User exists with correct password
  - Existing token found: `"existing_jwt_token"` with expiry `900`
- **Expected Output**: 
  - `TokenResponse` with AccessToken = "existing_jwt_token", ExpiresIn = 900
  - No new token created

#### 3.5 LoginOrRegisterAsync_NullRequest_ThrowsArgumentNullException
- **Input**: `null`
- **Expected Output**: `ArgumentNullException`

#### 3.6 LoginOrRegisterAsync_InvalidCredentials_ThrowsArgumentException
- **Test Cases**:
  | Username   | Password   |
  |------------|------------|
  | null       | "password" |
  | ""         | "password" |
  | " "        | "password" |
  | "username" | null       |
  | "username" | ""         |
  | "username" | " "        |
- **Expected Output**: `ArgumentException`

---

## Integration Tests

### 4. CalculationsApi Integration Tests (6 tests)

#### 4.1 PerformCalculation_ValidOperation_ReturnsCorrectResult
- **Test Cases**:
  | Operation  | Number1 | Number2 | Expected Result | Status Code |
  |------------|---------|---------|-----------------|-------------|
  | "add"      | 5.0     | 3.0     | 8.0             | 200 OK      |
  | "subtract" | 10.0    | 4.0     | 6.0             | 200 OK      |
  | "multiply" | 6.0     | 7.0     | 42.0            | 200 OK      |
  | "divide"   | 20.0    | 4.0     | 5.0             | 200 OK      |
- **Expected Output**: `CalculationResponse` with correct result and operation name

#### 4.2 PerformCalculation_DivisionByZero_ReturnsBadRequest
- **Input**: 
  - Number1: `10.0`
  - Number2: `0.0`
  - X-Operation header: `"divide"`
- **Expected Output**: HTTP 400 Bad Request

#### 4.3 PerformCalculation_MissingOperationHeader_ReturnsBadRequest
- **Input**: 
  - Number1: `5.0`
  - Number2: `3.0`
  - No X-Operation header
- **Expected Output**: HTTP 400 Bad Request

#### 4.4 PerformCalculation_InvalidOperation_ReturnsBadRequest
- **Input**: 
  - Number1: `5.0`
  - Number2: `3.0`
  - X-Operation header: `"invalid"`
- **Expected Output**: HTTP 400 Bad Request

#### 4.5 PerformCalculation_MissingNumber1_ReturnsBadRequest
- **Input**: 
  - Number1: `null`
  - Number2: `3.0`
  - X-Operation header: `"add"`
- **Expected Output**: HTTP 400 Bad Request

---

### 5. AuthenticationApi Integration Tests (6 tests)

#### 5.1 Login_NewUser_ReturnsTokenResponse
- **Input**: 
  - Username: `"testuser_<unique_guid>"`
  - Password: `"TestPassword123!"`
- **Expected Output**: 
  - HTTP 200 OK
  - `TokenResponse` with non-empty AccessToken, TokenType = "Bearer", ExpiresIn > 0

#### 5.2 Login_ExistingUserCorrectPassword_ReturnsTokenResponse
- **Input**: 
  - First request: Register user with username and password
  - Second request: Login with same credentials
- **Expected Output**: 
  - HTTP 200 OK
  - `TokenResponse` with valid token

#### 5.3 Login_ExistingUserWrongPassword_ReturnsUnauthorized
- **Input**: 
  - First request: Register user with `"CorrectPassword123!"`
  - Second request: Login with `"WrongPassword456!"`
- **Expected Output**: HTTP 401 Unauthorized

#### 5.4 Login_InvalidCredentials_ReturnsBadRequest
- **Test Cases**:
  | Username   | Password   | Expected Status |
  |------------|------------|-----------------|
  | null       | "password" | 400 Bad Request |
  | ""         | "password" | 400 Bad Request |
  | "username" | null       | 400 Bad Request |
  | "username" | ""         | 400 Bad Request |

#### 5.5 Login_TokenReuse_ReturnsSameToken
- **Input**: 
  - First request: Login with new user credentials
  - Second request: Login immediately with same credentials
- **Expected Output**: 
  - Both requests return HTTP 200 OK
  - Both responses contain the same AccessToken (token reuse)

---

## Test Infrastructure

### Database Cleanup
- **MongoDB**: All collections dropped after each test
- **Redis**: Database flushed after each test
- **Test Database**: `calculator_test_db` (separate from production)
- **Redis Prefix**: `test_` (separate from production)

### Test Configuration
- Uses `appsettings.Test.json` for test-specific settings
- Custom `WebApplicationFactory` with `IAsyncLifetime` for cleanup
- Each test runs in complete isolation

---

## Summary

- **Total Tests**: 41
  - **Unit Tests**: 29
    - PasswordHasher: 9 tests
    - CalculatorService: 12 tests
    - AuthService: 8 tests
  - **Integration Tests**: 12
    - CalculationsApi: 6 tests
    - AuthenticationApi: 6 tests

- **Coverage Areas**:
  - ✅ Security (password hashing and verification)
  - ✅ Business logic (calculations)
  - ✅ Authentication flow (login/registration)
  - ✅ API endpoints (HTTP requests/responses)
  - ✅ Error handling and validation
  - ✅ Edge cases and boundary conditions
