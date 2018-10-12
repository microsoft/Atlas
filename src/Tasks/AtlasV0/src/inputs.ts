import tl = require("vsts-task-lib/task");
import path = require("path");
import fs = require("fs");

const certFilePath: string = path.join(tl.getVariable('Agent.TempDirectory'), 'spnCert.pem');

export class ConnectedServiceAzureRM {

    connectedService: string;
    subscriptionId: string;
    subscriptionName: string;
    servicePrincipalId: string;
    environmentAuthorityUrl: string;
    tenantId: string;
    endpointUrl: string;
    environment: string;
    authorizationScheme: string;
    msiClientId: string;
    activeDirectoryServiceEndpointResourceId: string;
    azureKeyVaultServiceEndpointResourceId: string;
    azureKeyVaultDnsSuffix: string;
    authenticationType: string;
    servicePrincipalCertificate: string;
    servicePrincipalCertificatePath: string;
    servicePrincipalKey: string;

    constructor(connectedService: string) {
        this.connectedService = connectedService;
    }

    public async getParameters() : Promise<ConnectedServiceAzureRM> {
        this.subscriptionId = tl.getEndpointDataParameter(this.connectedService, 'subscriptionid', true);
        this.subscriptionName = tl.getEndpointDataParameter(this.connectedService, 'subscriptionname', true);
        this.servicePrincipalId = tl.getEndpointAuthorizationParameter(this.connectedService, 'serviceprincipalid', true);
        this.environmentAuthorityUrl = tl.getEndpointDataParameter(this.connectedService, 'environmentAuthorityUrl', true);
        this.tenantId = tl.getEndpointAuthorizationParameter(this.connectedService, 'tenantid', false);
        
        this.endpointUrl = tl.getEndpointUrl(this.connectedService, true);
        this.environment = tl.getEndpointDataParameter(this.connectedService, 'environment', true);
        this.authorizationScheme = tl.getEndpointAuthorizationScheme(this.connectedService, true);

        this.msiClientId = tl.getEndpointDataParameter(this.connectedService, 'msiclientId', true);
        this.activeDirectoryServiceEndpointResourceId = tl.getEndpointDataParameter(this.connectedService, 'activeDirectoryServiceEndpointResourceId', true);
        this.azureKeyVaultServiceEndpointResourceId = tl.getEndpointDataParameter(this.connectedService, 'AzureKeyVaultServiceEndpointResourceId', true);
        this.azureKeyVaultDnsSuffix = tl.getEndpointDataParameter(this.connectedService, 'AzureKeyVaultDnsSuffix', true);

        this.authenticationType = tl.getEndpointAuthorizationParameter(this.connectedService, 'authenticationType', true);

        tl.debug(JSON.stringify(this));

        let isServicePrincipalAuthenticationScheme = !this.authorizationScheme || this.authorizationScheme.toLowerCase() == 'serviceprincipal';
            if (isServicePrincipalAuthenticationScheme) {
                if(this.authenticationType && this.authenticationType == 'servicePrincipalCertificate') {
                    tl.debug('certificate spn endpoint');
                    this.servicePrincipalCertificate = tl.getEndpointAuthorizationParameter(this.connectedService, 'servicePrincipalCertificate', false);
                    this.servicePrincipalCertificatePath = certFilePath;
                    fs.writeFileSync(this.servicePrincipalCertificatePath, this.servicePrincipalCertificate);
                }
                else {
                    tl.debug('credentials spn endpoint');
                    this.servicePrincipalKey = tl.getEndpointAuthorizationParameter(this.connectedService, 'serviceprincipalkey', false);
                }
            }

        return this;
    }
}

export class TaskParameters {

    public command: string;
    public arguments: string;
    public azureSubscriptionEndpoint: ConnectedServiceAzureRM;

    public async getParameters() : Promise<TaskParameters> 
    {
        try {
            this.command = tl.getInput("command", true);
            this.arguments = tl.getInput("arguments", false);

            var connectedService = tl.getInput("azureSubscriptionEndpoint", true);

            this.azureSubscriptionEndpoint = await new ConnectedServiceAzureRM(connectedService).getParameters();

            tl.getInput("deploymentGroupEndpoint", false);

            return this;
        } catch (error) {
            throw new Error("Get parameters failed " + error.message);
        }
    }    
}
