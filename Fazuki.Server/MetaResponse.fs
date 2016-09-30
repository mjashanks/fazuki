namespace Fazuki.Server

open System

module Meta =

    let private makeMetaError id err = 
        () //{MessageId=id; Success=false; Error=err}	

    let internal GetErrorResponse (id:Guid) (error:ServerError) = 
        () //let metaMsg = makeMetaError id error
