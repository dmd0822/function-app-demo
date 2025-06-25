# Order Processing Durable Function

A robust order processing system built with Azure Durable Functions that orchestrates a complete e-commerce order workflow with automated processing, approval workflows, and error handling.

## Overview

This project demonstrates a real-world order processing scenario using Azure Durable Functions with the following features:

- **Automated Order Processing**: Multi-step workflow including inventory validation, payment processing, and shipment creation
- **Approval Workflow**: High-value orders (>$1000) require manager approval with timeout handling
- **Error Handling**: Comprehensive compensation logic for failed operations
- **External Events**: Support for external approval events
- **Monitoring**: Detailed logging and status tracking throughout the process

## Architecture

The application follows the Durable Functions pattern with:

- **HTTP Triggers**: REST API endpoints for starting orders and handling approvals
- **Orchestrator**: Main workflow coordinator that manages the order processing steps
- **Activities**: Individual business logic functions for each processing step
- **External Events**: Manager approval mechanism with timeout handling

## Features

### Order Processing Workflow

1. **Inventory Validation** - Checks product availability
2. **Payment Processing** - Handles payment transactions
3. **Shipment Creation** - Creates shipping records
4. **Email Confirmation** - Sends order confirmation
5. **Manager Approval** (for orders >$1000) - Waits for external approval with 24-hour timeout
6. **Order Finalization** - Completes the order process

### API Endpoints

- `POST /api/orders/start` - Start a new order processing workflow
- `GET /api/orders/{instanceId}/status` - Get order status
- `POST /api/orders/approve` - Approve/reject an order
- `DELETE /api/orders/{instanceId}/terminate` - Terminate an order

## Prerequisites

- [.NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (for local development)

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd OrderProcessingDurableFunction
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Start Azurite

Start the Azure Storage Emulator for local development:

```bash
azurite --silent --location . --debug .
```

### 4. Configure Local Settings

Ensure your [`local.settings.json`](local.settings.json) is properly configured with:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```

### 5. Build and Run

```bash
dotnet build
func start
```

The function app will start on `http://localhost:7071`

## Usage Examples

### Start a Low-Value Order (No Approval Required)

```http
POST http://localhost:7071/api/orders/start
Content-Type: application/json

{
    "customerEmail": "customer@example.com",
    "amount": 500.00,
    "productName": "Wireless Headphones",
    "quantity": 1
}
```

### Start a High-Value Order (Requires Approval)

```http
POST http://localhost:7071/api/orders/start
Content-Type: application/json

{
    "customerEmail": "bigcustomer@example.com",
    "amount": 1500.00,
    "productName": "Laptop Computer",
    "quantity": 1
}
```

### Check Order Status

```http
GET http://localhost:7071/api/orders/{instanceId}/status
```

### Approve an Order

```http
POST http://localhost:7071/api/orders/approve
Content-Type: application/json

{
    "instanceId": "{instanceId}",
    "approved": true
}
```

## Project Structure

```
OrderProcessingDurableFunction/
├── Models.cs                          # Data models for orders and requests
├── OrderProcessingHttpTrigger.cs      # HTTP trigger functions (API endpoints)
├── OrderProcessingOrchestrator.cs     # Main orchestration logic
├── OrderProcessingActivities.cs       # Individual activity functions
├── Program.cs                         # Application entry point and configuration
├── host.json                         # Function app configuration
├── local.settings.json               # Local development settings
├── test-requests.http                # Sample HTTP requests for testing
└── OrderProcessingDurableFunction.csproj  # Project file
```

## Key Components

### Models

- **`OrderData`** - Complete order information including ID, customer details, and product information
- **`OrderStartRequest`** - Request payload for starting new orders
- **`ApprovalRequest`** - Request payload for order approval/rejection

### HTTP Triggers

- **`StartOrderProcessing`** - Initiates new order workflows
- **`GetOrderStatus`** - Retrieves current order status
- **`ApproveOrder`** - Handles manager approval decisions
- **`TerminateOrder`** - Cancels running orders

### Activities

- **`ValidateInventory`** - Simulates inventory validation with 90% success rate
- **`ProcessPayment`** - Simulates payment processing with transaction ID generation
- **`CreateShipment`** - Creates shipment records with tracking numbers
- **`SendConfirmationEmail`** - Sends order confirmation emails
- **`FinalizeOrder`** - Completes the order process
- **`CancelOrder`** - Handles order cancellation (compensation logic)

## Configuration

### Function App Settings

The [`host.json`](host.json) file configures:

- Durable Task Hub settings
- Concurrency limits for orchestrators and activities
- Azure Storage provider configuration
- Logging levels and Application Insights integration

### Development Settings

Local development uses Azurite for storage emulation. See [`local.settings.json`](local.settings.json) for configuration.

## Testing

Use the provided [`test-requests.http`](test-requests.http) file with REST Client extensions in VS Code or similar tools to test the API endpoints.

### Test Scenarios

1. **Normal Order Flow** - Test with amount < $1000
2. **Approval Required Flow** - Test with amount > $1000
3. **Order Approval** - Test manager approval process
4. **Order Rejection** - Test order rejection handling
5. **Timeout Scenarios** - Test 24-hour approval timeouts

## Deployment

### Build for Production

```bash
dotnet publish --configuration Release
```

### Deploy to Azure

1. Create an Azure Function App with .NET 9.0 runtime
2. Configure Azure Storage account for durable functions
3. Deploy using Azure CLI, Visual Studio, or GitHub Actions

## Monitoring and Logging

The application includes comprehensive logging at each step:

- Order processing start/completion
- Individual activity execution
- Replay detection for troubleshooting
- Error handling and compensation logic
- External event handling

Use Application Insights for production monitoring and diagnostics.

## Error Handling

The orchestrator includes robust error handling:

- **Inventory Failures** - Graceful order rejection
- **Payment Failures** - Automatic compensation
- **Timeout Handling** - 24-hour approval timeouts
- **Activity Failures** - Retry policies and compensation logic

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Additional Resources

- [Azure Durable Functions Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/durable/)
- [Durable Functions Patterns](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview)
- [Azure Functions Best Practices](https://docs.microsoft.com/en-us/azure/azure-functions/functions-best-practices)